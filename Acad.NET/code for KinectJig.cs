// code for KinectJig:
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Linq;
using System.IO;
using System;
 
namespace KinectSamples
{
  public class ColoredPoint3d
  {
    public double X, Y, Z;
    public int R, G, B;
  }
 
  public abstract class KinectJig : DrawJig
  {
    // To stop the running jig by sending a cancel request
 
    [DllImport("acad.exe", CharSet = CharSet.Auto,
      CallingConvention = CallingConvention.Cdecl,
      EntryPoint = "?acedPostCommand@@YAHPB_W@Z"
     )]
	 
    extern static private int acedPostCommand(string strExpr);
 
    // We need our Kinect sensor
 
    private Runtime _kinect = null;
 
    // And audio array
 
    private KinectAudioSource _audio;
 
    // Microsoft Speech recognition engine
 
    private SpeechRecognitionEngine _sre;
 
    // With the images collected by it
 
    private ImageFrame _depth = null;
    private ImageFrame _video = null;
 
    // An offset value we use to move the mouse back
    // and forth by one screen unit
 
    private int _offset;
 
    // Selected color, for whatever usage makes sense
    // in child classes
 
    private short _color;
 
    internal short ColorIndex
    {
      get { return _color; }
    }
 
    // Sampling value to reduce point set on capture
 
    internal static short Sampling
    {
      get
      {
        try
        {
          return (short)Application.GetSystemVariable("KINSAMP");
        }
        catch
        {
          return 50;
        }
      }
    }
 
    // Extents to filter points
 
    private static Extents3d? _ext = null;
 
    public static Extents3d? Extents
    {
      get { return _ext; }
      set { _ext = value; }
    }
 
    public KinectJig()
    {
      // Initialise the various members
 
      _offset = 1;
      _color = 2;
 
      _kinect = Runtime.Kinects[0];
 
      _kinect.VideoFrameReady +=
        new EventHandler<ImageFrameReadyEventArgs>(
          OnVideoFrameReady
        );
      _kinect.DepthFrameReady +=
        new EventHandler<ImageFrameReadyEventArgs>(
          OnDepthFrameReady
        );
      _kinect.SkeletonFrameReady +=
        new EventHandler<SkeletonFrameReadyEventArgs>(
          OnSkeletonFrameReady
        );
    }
 
    public virtual void OnDepthFrameReady(
      object sender, ImageFrameReadyEventArgs e
    )
    {
      _depth = e.ImageFrame;
    }
 
    public virtual void OnVideoFrameReady(
      object sender, ImageFrameReadyEventArgs e
    )
    {
      _video = e.ImageFrame;
    }
 
    public virtual void OnSkeletonFrameReady(
      object sender, SkeletonFrameReadyEventArgs e
    )
    {
    }
 
    void OnSpeechRecognized(
      object sender, SpeechRecognizedEventArgs e
    )
    {
      // Ignore if we don't have a high degree of confidence
 
      if (e.Result.Confidence < 0.8)
        return;
 
      // Set the color property based om the text input
 
      switch (e.Result.Text.ToUpperInvariant())
      {
        case "RED":
          _color = 1;
          break;
        case "YELLOW":
          _color = 2;
          break;
        case "GREEN":
          _color = 3;
          break;
        case "CYAN":
          _color = 4;
          break;
        case "BLUE":
          _color = 5;
          break;
        case "MAGENTA":
          _color = 6;
          break;
        default:
          _color = 7;
          break;
      }
    }
 
    public void StartSensor()
    {
      if (_kinect != null)
      {
        // We still need to enable skeletal tracking
        // in order to map to "real" space, even
        // if we're not actually getting skeleton data
 
        _kinect.Initialize(
          RuntimeOptions.UseDepth |
          RuntimeOptions.UseColor |
          RuntimeOptions.UseSkeletalTracking
        );
        _kinect.VideoStream.Open(
          ImageStreamType.Video, 2,
          ImageResolution.Resolution640x480,
          ImageType.Color
        );
        _kinect.DepthStream.Open(
          ImageStreamType.Depth, 2,
          ImageResolution.Resolution640x480,
          ImageType.Depth
        );
 
        InitializeSpeech();
        StartSpeech();
      }
    }
 
    private static RecognizerInfo GetKinectRecognizer()
    {
      Func<RecognizerInfo, bool> matchingFunc =
        r =>
        {
          string value;
          r.AdditionalInfo.TryGetValue("Kinect", out value);
          return
            "True".Equals(
              value,
              StringComparison.InvariantCultureIgnoreCase
            ) &&
            "en-US".Equals(
              r.Culture.Name,
              StringComparison.InvariantCultureIgnoreCase
            );
        };
      return
        SpeechRecognitionEngine.InstalledRecognizers().Where(
          matchingFunc
        ).FirstOrDefault();
    }
 
    private void InitializeSpeech()
    {
      RecognizerInfo ri = GetKinectRecognizer();
      if (ri == null)
      {
        Editor ed =
          Application.DocumentManager.MdiActiveDocument.Editor;
 
        ed.WriteMessage(
          "There was a problem initializing Speech Recognition. " +
          "Ensure you have the Microsoft Speech SDK installed " +
          "and configured."
        );
      }
 
      try
      {
        _sre = new SpeechRecognitionEngine(ri.Id);
      }
      catch
      {
        Editor ed =
          Application.DocumentManager.MdiActiveDocument.Editor;
 
        ed.WriteMessage(
          "There was a problem initializing Speech Recognition. " +
          "Ensure you have the Microsoft Speech SDK installed " +
          "and configured."
        );
      }
 
      // Populate our word choices
 
      Choices colors = new Choices();
      colors.Add("red");
      colors.Add("green");
      colors.Add("blue");
      colors.Add("yellow");
      colors.Add("magenta");
      colors.Add("cyan");
 
      // Create a GrammarBuilder from them
 
      GrammarBuilder gb = new GrammarBuilder();
      gb.Culture = ri.Culture;
      gb.Append(colors);
 
      // Create the actual Grammar instance, and then load it
      // into the speech recognizer
 
      Grammar g = new Grammar(gb);
      _sre.LoadGrammar(g);
 
      // Attach our event handler for recognized commands
      // We won't worry about rejected or hypothesized callbacks
 
      _sre.SpeechRecognized += OnSpeechRecognized;
    }
 
    private void StartSpeech()
    {
      try
      {
        // Create and setup our audio source
 
        _audio = new KinectAudioSource();
        _audio.SystemMode = SystemMode.OptibeamArrayOnly;
        _audio.FeatureMode = true;
        _audio.AutomaticGainControl = false;
        _audio.MicArrayMode = MicArrayMode.MicArrayAdaptiveBeam;
 
        // Get the audio stream and pass it to the
        // speech recognition engine
 
        Stream kinectStream = _audio.Start();
 
        _sre.SetInputToAudioStream(
          kinectStream,
          new SpeechAudioFormatInfo(
            EncodingFormat.Pcm, 16000, 16, 1,
            32000, 2, null
          )
        );
        _sre.RecognizeAsync(RecognizeMode.Multiple);
      }
      catch
      {
        Editor ed =
          Application.DocumentManager.MdiActiveDocument.Editor;
        ed.WriteMessage(
          "There was a problem initializing the KinectAudioSource." +
          " Ensure you have the Kinect SDK installed correctly."
        );
      }
    }
 
    public void StopSensor()
    {
      if (_kinect != null)
      {
        _audio.Stop();
        _sre.RecognizeAsyncCancel();
        _sre.RecognizeAsyncStop();
        _audio.Dispose();
        _kinect.Uninitialize();
        _kinect = null;
      }
    }
 
    protected virtual SamplerStatus SamplerData()
    {
      return SamplerStatus.Cancel;
    }
 
    protected virtual bool WorldDrawData(WorldDraw draw)
    {
      return false;
    }
 
    protected override SamplerStatus Sampler(JigPrompts prompts)
    {
      // We don't really need a point, but we do need some
      // user input event to allow us to loop, processing
      // for the Kinect input
 
      PromptPointResult ppr =
        prompts.AcquirePoint("\nClick to capture: ");
      if (ppr.Status == PromptStatus.OK)
      {
        return SamplerData();
      }
      return SamplerStatus.Cancel;
    }
 
    protected override bool WorldDraw(WorldDraw draw)
    {
      return WorldDrawData(draw);
    }
 
    public void ForceMessage()
    {
      // Let's move the mouse slightly to avoid having
      // to do it manually to keep the input coming
 
      System.Drawing.Point pt =
        System.Windows.Forms.Cursor.Position;
      System.Windows.Forms.Cursor.Position =
        new System.Drawing.Point(
          pt.X, pt.Y + _offset
        );
      _offset = -_offset;
    }
 
    public List<ColoredPoint3d> GeneratePointCloud(
      int sampling, bool useColor = false)
    {
      return GeneratePointCloud(
        _kinect, _depth, _video, sampling, useColor
      );
    }
 
    // Generate a point cloud from depth and RGB data
 
    internal static List<ColoredPoint3d> GeneratePointCloud(
      Runtime kinect, ImageFrame depth, ImageFrame video,
      int sampling, bool withColor = false
    )
    {
      // We will return a list of our ColoredPoint3d objects
 
      List<ColoredPoint3d> res = new List<ColoredPoint3d>();
 
      // Let's start by determining the dimensions of the
      // respective images
 
      int depHeight = depth.Image.Height;
      int depWidth = depth.Image.Width;
      int vidHeight = video.Image.Height;
      int vidWidth = video.Image.Width;
 
      // For the sake of this initial implementation, we
      // expect them to be the same size. But this should not
      // actually need to be a requirement
 
      if (vidHeight != depHeight || vidWidth != depWidth)
      {
        Application.DocumentManager.MdiActiveDocument.
        Editor.WriteMessage(
          "\nVideo and depth images are of different sizes."
        );
        return null;
      }
 
      // Depth and color data for each pixel
 
      Byte[] depthData = depth.Image.Bits;
      Byte[] colorData = video.Image.Bits;
 
      // Loop through the depth information - we process two
      // bytes at a time
 
      for (int i = 0; i < depthData.Length; i += (2 * sampling))
      {
        // The depth pixel is two bytes long - we shift the
        // upper byte by 8 bits (a byte) and "or" it with the
        // lower byte
 
        int depthPixel = (depthData[i + 1] << 8) | depthData[i];
 
        // The x and y positions can be calculated using modulus
        // division from the array index
 
        int x = (i / 2) % depWidth;
        int y = (i / 2) / depWidth;
 
        // The x and y we pass into DepthImageToSkeleton() need to
        // be normalised (between 0 and 1), so we divide by the
        // width and height of the depth image, respectively
 
        // As we're using UseDepth (not UseDepthAndPlayerIndex) in
        // the depth sensor settings, we also need to shift the
        // depth pixel by 3 bits
 
        Vector v =
          kinect.SkeletonEngine.DepthImageToSkeleton(
            ((float)x) / ((float)depWidth),
            ((float)y) / ((float)depHeight),
            (short)(depthPixel << 3)
          );
 
        // A zero value for Z means there is no usable depth for
        // that pixel
 
        if (v.Z > 0)
        {
          // Create a ColorVector3 to store our XYZ and RGB info
          // for a pixel
 
          ColoredPoint3d cv = new ColoredPoint3d();
          cv.X = v.X;
          cv.Y = v.Z;
          cv.Z = v.Y;
 
          // Only calculate the colour when it's needed (as it's
          // now more expensive, albeit more accurate)
 
          if (withColor)
          {
            // Get the colour indices for that particular depth
            // pixel. We once again need to shift the depth pixel
            // and also need to flip the x value (as UseDepth means
            // it is mirrored on X) and do so on the basis of
            // 320x240 resolution (so we divide by 2, assuming
            // 640x480 is chosen earlier), as that's what this
            // function expects. Phew!
 
            int colorX, colorY;
            kinect.NuiCamera.
              GetColorPixelCoordinatesFromDepthPixel(
                video.Resolution, video.ViewArea,
                320 - (x / 2), (y / 2), (short)(depthPixel << 3),
                out colorX, out colorY
              );
 
            // Make sure both indices are within bounds
 
            colorX = Math.Max(0, Math.Min(vidWidth - 1, colorX));
            colorY = Math.Max(0, Math.Min(vidHeight - 1, colorY));
 
            // Extract the RGB data from the appropriate place
            // in the colour data
 
            int colIndex = 4 * (colorX + (colorY * vidWidth));
            cv.B = (byte)(colorData[colIndex + 0]);
            cv.G = (byte)(colorData[colIndex + 1]);
            cv.R = (byte)(colorData[colIndex + 2]);
          }
          else
          {
            // If we don't need colour information, just set each
            // pixel to white
 
            cv.B = 255;
            cv.G = 255;
            cv.R = 255;
          }
 
          // Add our pixel data to the list to return
 
          res.Add(cv);
        }
      }
 
      // Apply a bounding box filter, if one is defined
 
      if (_ext.HasValue)
      {
        // Use LINQ to get the points within the
        // bounding box
 
        var vecSet =
          from ColoredPoint3d vec in res
          where
            vec.X > _ext.Value.MinPoint.X &&
            vec.X < _ext.Value.MaxPoint.X &&
            vec.Y > _ext.Value.MinPoint.Y &&
            vec.Y < _ext.Value.MaxPoint.Y &&
            vec.Z > _ext.Value.MinPoint.Z &&
            vec.Z < _ext.Value.MaxPoint.Z
          select vec;
 
        // Convert our IEnumerable<> into a List<>
 
        res = vecSet.ToList<ColoredPoint3d>();
      }
 
      return res;
    }
 
    // Save the provided point cloud to a specific file
 
    internal static void ExportPointCloud(
      List<ColoredPoint3d> vecs, string filename
    )
    {
      if (vecs.Count > 0)
      {
        using (StreamWriter sw = new StreamWriter(filename))
        {
          // For each pixel, write a line to the text file:
          // X, Y, Z, R, G, B
 
          foreach (ColoredPoint3d pt in vecs)
          {
            sw.WriteLine(
              "{0}, {1}, {2}, {3}, {4}, {5}",
              pt.X, pt.Y, pt.Z, pt.R, pt.G, pt.B
            );
          }
        }
      }
    }
 
    // Translate from Skeleton Space to WCS
 
    internal static Point3d PointFromVector(Vector v)
    {
      // Rather than just return a point, we're effectively
      // transforming it to the drawing space: flipping the
      // Y and Z axes (which makes it consistent with the
      // point cloud, and makes sure Z is actually up - from
      // the Kinect's perspective Y is up), and reversing
      // the X axis (which is the result of choosing UseDepth
      // rather than UseDepthAndPlayerIndex)
 
      return new Point3d(-v.X, v.Z, v.Y);
    }
 
    // Cancel the running jig
 
    internal static void CancelJig()
    {
      acedPostCommand("CANCELCMD");
    }
 
    // Write the provided point cloud to file, then chain
    // the commands needed to import it into AutoCAD
 
    public void WriteAndImportPointCloud(
      Document doc, List<ColoredPoint3d> vecs
    )
    {
      Editor ed = doc.Editor;
 
      // We'll store most local files in the temp folder.
      // We get a temp filename, delete the file and
      // use the name for our folder
 
      string localPath = Path.GetTempFileName();
      File.Delete(localPath);
      Directory.CreateDirectory(localPath);
      localPath += "\\";
 
      // Paths for our temporary files
 
      string txtPath = localPath + "points.txt";
      string lasPath = localPath + "points.las";
 
      // Our PCG file will be stored under My Documents
 
      string outputPath =
        Environment.GetFolderPath(
          Environment.SpecialFolder.MyDocuments
        ) + "\\Kinect Point Clouds\\";
 
      if (!Directory.Exists(outputPath))
        Directory.CreateDirectory(outputPath);
 
      // We'll use the title as a base filename for the PCG,
      // but will use an incremented integer to get an unused
      // filename
 
      int cnt = 0;
      string pcgPath;
      do
      {
        pcgPath =
          outputPath + "Kinect" +
          (cnt == 0 ? "" : cnt.ToString()) + ".pcg";
        cnt++;
      }
      while (File.Exists(pcgPath));
 
      // The path to the txt2las tool will be the same as the
      // executing assembly (our DLL)
 
      string exePath =
        Path.GetDirectoryName(
          Assembly.GetExecutingAssembly().Location
        ) + "\\";
 
      if (!File.Exists(exePath + "txt2las.exe"))
      {
        ed.WriteMessage(
          "\nCould not find the txt2las tool: please make sure " +
          "it is in the same folder as the application DLL."
        );
        return;
      }
 
      // Export our point cloud from the jig
 
      ed.WriteMessage(
        "\nSaving TXT file of the captured points.\n"
      );
 
      ExportPointCloud(vecs, txtPath);
 
      // Use the txt2las utility to create a .LAS
      // file from our text file
 
      ed.WriteMessage(
        "\nCreating a LAS from the TXT file.\n"
      );
 
      ProcessStartInfo psi =
        new ProcessStartInfo(
          exePath + "txt2las",
          "-i \"" + txtPath +
          "\" -o \"" + lasPath +
          "\" -parse xyzRGB"
        );
      psi.CreateNoWindow = false;
      psi.WindowStyle = ProcessWindowStyle.Hidden;
 
      // Wait up to 20 seconds for the process to exit
 
      try
      {
        using (Process p = Process.Start(psi))
        {
          p.WaitForExit();
        }
      }
      catch
      { }
 
      // If there's a problem, we return
 
      if (!File.Exists(lasPath))
      {
        ed.WriteMessage(
          "\nError creating LAS file."
        );
        return;
      }
 
      File.Delete(txtPath);
 
      ed.WriteMessage(
        "Indexing the LAS and attaching the PCG.\n"
      );
 
      // Index the .LAS file, creating a .PCG
 
      string lasLisp = lasPath.Replace('\\', '/'),
              pcgLisp = pcgPath.Replace('\\', '/');
 
      doc.SendStringToExecute(
        "(command \"_.POINTCLOUDINDEX\" \"" +
          lasLisp + "\" \"" +
          pcgLisp + "\")(princ) ",
        false, false, false
      );
 
      // Attach the .PCG file
 
      doc.SendStringToExecute(
        "_.WAITFORFILE \"" +
        pcgLisp + "\" \"" +
        lasLisp + "\" " +
        "(command \"_.-POINTCLOUDATTACH\" \"" +
        pcgLisp +
        "\" \"0,0\" \"1\" \"0\")(princ) ",
        false, false, false
      );
 
      doc.SendStringToExecute(
        "_.-VISUALSTYLES _C _Realistic ",
        false, false, false
      );
    }
  }
 
  public class KinectCommands
  {
    // Set the clipping volume for the current point cloud
 
    [CommandMethod("ADNPLUGINS", "KINBOUNDS", CommandFlags.Modal)]
    public void SetBoundingBox()
    {
      Document doc =
        Autodesk.AutoCAD.ApplicationServices.
          Application.DocumentManager.MdiActiveDocument;
      Editor ed = doc.Editor;
 
      // Ask the user to select an entity
 
      PromptEntityOptions peo =
        new PromptEntityOptions(
          "\nSelect entity to define bounding box"
        );
      peo.AllowNone = true;
      peo.Keywords.Add("None");
      peo.Keywords.Default = "None";
 
      PromptEntityResult per = ed.GetEntity(peo);
 
      if (per.Status != PromptStatus.OK)
        return;
 
      // If "None" selected, clear the bounding box
 
      if (per.Status == PromptStatus.None ||
          per.StringResult == "None")
      {
        KinectJig.Extents = null;
        ed.WriteMessage("\nBounding box cleared.");
        return;
      }
 
      // Otherwise open the entity and gets its extents
 
      Transaction tr =
        doc.TransactionManager.StartTransaction();
      using (tr)
      {
        Entity ent =
          tr.GetObject(per.ObjectId, OpenMode.ForRead)
            as Entity;
        if (ent != null)
          KinectJig.Extents = ent.Bounds;
 
        ed.WriteMessage(
          "\nBounding box set to {0}", KinectJig.Extents
        );
        tr.Commit();
      }
    }
 
    // A command which waits for a particular PCG file to exist
 
    [CommandMethod(
      "ADNPLUGINS", "WAITFORFILE", CommandFlags.NoHistory
     )]
    public void WaitForFileToExist()
    {
      Document doc =
        Application.DocumentManager.MdiActiveDocument;
      Editor ed = doc.Editor;
      HostApplicationServices ha =
        HostApplicationServices.Current;
 
      PromptResult pr = ed.GetString("Enter path to PCG: ");
      if (pr.Status != PromptStatus.OK)
        return;
      string pcgPath = pr.StringResult.Replace('/', '\\');
 
      pr = ed.GetString("Enter path to LAS: ");
      if (pr.Status != PromptStatus.OK)
        return;
      string lasPath = pr.StringResult.Replace('/', '\\');
 
      ed.WriteMessage(
        "\nWaiting for PCG creation to complete...\n"
      );
 
      // Check the write time for the PCG file...
      // if it hasn't been written to for at least half a second,
      // then we try to use a file lock to see whether the file
      // is accessible or not
 
      const int ticks = 50;
      TimeSpan diff;
      bool cancelled = false;
 
      // First loop is to see when writing has stopped
      // (better than always throwing exceptions)
 
      while (true)
      {
        if (File.Exists(pcgPath))
        {
          DateTime dt = File.GetLastWriteTime(pcgPath);
          diff = DateTime.Now - dt;
          if (diff.Ticks > ticks)
            break;
        }
        System.Windows.Forms.Application.DoEvents();
        if (HostApplicationServices.Current.UserBreak())
        {
          cancelled = true;
          break;
        }
      }
 
      // Second loop will wait until file is finally accessible
      // (by calling a function that requests an exclusive lock)
 
      if (!cancelled)
      {
        int inacc = 0;
        while (true)
        {
          if (IsFileAccessible(pcgPath))
            break;
          else
            inacc++;
          System.Windows.Forms.Application.DoEvents();
          if (HostApplicationServices.Current.UserBreak())
          {
            cancelled = true;
            break;
          }
        }
        ed.WriteMessage("\nFile inaccessible {0} times.", inacc);
 
        try
        {
          CleanupTmpFiles(lasPath);
        }
        catch
        { }
      }
    }
 
    // Return whether a file is accessible
 
    internal bool IsFileAccessible(string filename)
    {
      // If the file can be opened for exclusive access it means
      // the file is accesible
      try
      {
        FileStream fs =
          File.Open(
            filename, FileMode.Open,
            FileAccess.Read, FileShare.None
          );
        using (fs)
        {
          return true;
        }
      }
      catch (IOException)
      {
        return false;
      }
    }
 
    // Remove any temporary files from the point cloud import
 
    internal void CleanupTmpFiles(string txtPath)
    {
      if (File.Exists(txtPath))
        File.Delete(txtPath);
      Directory.Delete(
        Path.GetDirectoryName(txtPath)
      );
    }
  }
}

// Here's the updated implementation for created segmented solids:
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.GraphicsInterface;
using Microsoft.Research.Kinect.Nui;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System;
 
namespace KinectSamples
{
  public class KinectSegmentedSolidsJig : KinectPointCloudJig
  {
    // Our transient solids (cursor sphere & tube) are yellow
 
    const short transSolColor = 2;
 
    // Our final solids will be green
 
    const short finalSolColor = 3;
 
    // A transaction and database to add solids
 
    private Transaction _tr;
    private Document _doc;
 
    // A list of vertices to draw between
    // (we use this for the final polyline creation)
 
    private Point3dCollection _vertices;
    private int _lastDrawnVertex;
 
    // Entities to create our solid
 
    private DBObjectCollection _created;
 
    // The radius of the profile circle to create
 
    private double _profRad;
 
    // The location at which to draw a sphere when resizing
 
    private Point3d _resizeLocation;
 
    // The approximate length of each swept segment
    // (as a multiple of the radius)
 
    private double _segFactor;
 
    // Flags to indicate Kinect gesture modes
 
    private bool _resizing;     // Drawing mode active
    private bool _drawing;     // Drawing mode active
    private bool _finished;    // Finished - want to exit
 
    public bool Finished
    {
      get { return _finished; }
    }
 
    public KinectSegmentedSolidsJig(
      Document doc, Transaction tr, double profRad, double factor
    )
    {
      // Initialise the various members
 
      _doc = doc;
      _tr = tr;
      _vertices = new Point3dCollection();
      _lastDrawnVertex = -1;
      _resizing = false;
      _drawing = false;
      _finished = false;
      _created = new DBObjectCollection();
      _profRad = profRad;
      _segFactor = factor;
    }
 
    public override void OnSkeletonFrameReady(
      object sender, SkeletonFrameReadyEventArgs e
    )
    {
      SkeletonFrame s = e.SkeletonFrame;
 
      if (!_finished)
      {
        foreach (SkeletonData data in s.Skeletons)
        {
          if (data.TrackingState == SkeletonTrackingState.Tracked)
          {
            Point3d leftHip =
              PointFromVector(
                data.Joints[JointID.HipLeft].Position
              );
            Point3d leftHand =
              PointFromVector(
                data.Joints[JointID.HandLeft].Position
              );
            Point3d rightHand =
              PointFromVector(
                data.Joints[JointID.HandRight].Position
              );
 
            if (
              leftHand.DistanceTo(Point3d.Origin) > 0 &&
              rightHand.DistanceTo(Point3d.Origin) > 0 &&
              leftHand.DistanceTo(rightHand) < 0.03)
            {
              // Hands are less than 3cm from each other
 
              _drawing = false;
              _resizing = false;
              _finished = true;
            }
            else
            {
              // Hands are within 10cm of each other vertically and
              // both hands are above the waist, so we resize the
              // profile radius
 
              _resizing =
                (leftHand.Z > leftHip.Z &&
                 rightHand.Z > leftHip.Z &&
                 Math.Abs(leftHand.Z - rightHand.Z) < 0.1);
 
              // If the left hand is below the waist, we draw
 
              _drawing = (leftHand.Z < leftHip.Z);
            }
 
            if (_resizing)
            {
              // If resizing, set some data to help draw
              // a sphere where we're resizing
 
              Vector3d vec = (leftHand - rightHand) / 2;
              _resizeLocation = rightHand + vec;
              _profRad = vec.Length;
            }
 
            if (_drawing)
            {
              // If we have at least one prior vertex...
 
              if (_vertices.Count > 0)
              {
                // ... check whether we're a certain distance away
                // from the last one before adding it (this smooths
                // off the jitters of adding every point)
 
                Point3d lastVert = _vertices[_vertices.Count - 1];
                if (lastVert.DistanceTo(rightHand) > _profRad * 4)
                {
                  // Add the new vertex to our list
 
                  _vertices.Add(rightHand);
                }
              }
              else
              {
                // Add the first vertex to our list
 
                _vertices.Add(rightHand);
              }
            }
            break;
          }
        }
      }
    }
 
    public void Cleanup()
    {
      _vertices.Clear();
 
      foreach (DBObject obj in _created)
      {
        obj.Dispose();        
      }
      _created.Clear();
    }
 
    private bool GenerateTube(
      double profRad, Point3dCollection pts, out Solid3d sol
    )
    {
      bool readyToBreak;
 
      // Let's start by creating our spline path
 
      using (Spline path = new Spline(pts, 0, 0.0))
      {
        double pathLen = path.GetDistanceAtParameter(path.EndParam);
        readyToBreak = (pathLen > _profRad * _segFactor);
 
        // And our sweep profile
 
        Circle profile =
          new Circle(pts[0], pts[1] - pts[0], profRad);
        using (profile)
        {
          // Then our sweep options
 
          SweepOptionsBuilder sob = new SweepOptionsBuilder();
 
          // Align the entity to sweep to the path
 
          sob.Align =
            SweepOptionsAlignOption.AlignSweepEntityToPath;
 
          // The base point is the start of the path
 
          sob.BasePoint = path.StartPoint;
 
          // The profile will rotate to follow the path
 
          sob.Bank = true;
          using (SweepOptions sweepOpts = sob.ToSweepOptions())
          {
            sol = new Solid3d();
            sol.ColorIndex = ColorIndex;
 
            // Sweep our profile along our path
 
            sol.CreateSweptSolid(profile, path, sweepOpts);
          }
        }
      }
      _lastDrawnVertex = pts.Count - 1;
 
      return readyToBreak;
    }
 
    protected override SamplerStatus SamplerData()
    {
      if (_finished)
      {
        CancelJig();
        return SamplerStatus.Cancel;
      }
 
      // If not finished, but stopped drawing, add the
      // geometry that was previously drawn to the database
 
      if (!_drawing &&
            (_created.Count > 0 || _vertices.Count > 0)
        )
      {
        AddSolidOrPath();
      }
 
      return base.SamplerData();
    }
 
    // Helper functions to extract/blank portions of our
    // vertex list (when we want to draw the beginning of it)
 
    private void ClearAllButLast(Point3dCollection pts, int n)
    {
      while (pts.Count > n)
      {
        pts.RemoveAt(0);
      }
      _lastDrawnVertex = -1;
    }
 
    private Point3dCollection GetAllButLast(
      Point3dCollection pts, int n
    )
    {
      Point3dCollection res = new Point3dCollection();
      for (int i = 0; i < pts.Count - n; i++)
      {
        res.Add(pts[i]);
      }
      return res;
    }
 
    protected override bool WorldDrawData(WorldDraw draw)
    {
      if (!base.WorldDrawData(draw))
        return false;
 
      short origCol = draw.SubEntityTraits.Color;
 
      if (_resizing)
      {
        using (Solid3d sphere = new Solid3d())
        {
          try
          {
            sphere.CreateSphere(_profRad);
 
            if (sphere != null)
            {
              sphere.TransformBy(
                Matrix3d.Displacement(
                  _resizeLocation - Point3d.Origin
                )
              );
 
              // Draw the cursor
 
              draw.SubEntityTraits.Color = ColorIndex;
              sphere.WorldDraw(draw);
            }
          }
          catch { }
          finally
          {
            draw.SubEntityTraits.Color = origCol;
          }
        }
        return true;
      }
 
      // If we're currently drawing...
 
      if (_drawing)
      {
        Solid3d sol = null;
        try
        {
          // If we have vertices that haven't yet been drawn...
 
          if (_vertices.Count > 1 //&&
              //_vertices.Count - 1 > _lastDrawnVertex
            )
          {
            // ... generate a tube
 
            if (GenerateTube(_profRad, _vertices, out sol))
            {
              // We now need to break the pipe...
 
              // If it was created, add it to our list to draw
 
              _created.Add(sol);
              sol = null;
 
              // Clear all but the last vertex to draw from
              // next time
 
              ClearAllButLast(_vertices, 1);
            }
          }
        }
        catch
        {
          // If the tube generation failed...
 
          if (sol != null)
          {
            sol.Dispose();
          }
 
          // Loop, creating the most recent successful tube we can
 
          bool succeeded = false;
          int n = 1;
 
          do
          {
            try
            {
              // Generate the previous, working tube using all
              // but the last points (if it fails, one more is
              // excluded per iteration, until we get a working
              // tube)
 
              GenerateTube(
                _profRad, GetAllButLast(_vertices, n++), out sol
              );
 
              _created.Add(sol);
              sol = null;
              succeeded = true;
            }
            catch { }
          }
          while (!succeeded && n < _vertices.Count);
 
          if (succeeded)
          {
            ClearAllButLast(_vertices, n - 1);
 
            if (_vertices.Count > 1)
            {
              try
              {
                // And generate a tube for the remaining vertices
 
                GenerateTube(_profRad, _vertices, out sol);
              }
              catch
              {
                succeeded = false;
              }
            }
          }
 
          if (!succeeded && sol != null)
          {
            sol.Dispose();
            sol = null;
          }
        }
 
        // Draw our solid(s)
 
        draw.SubEntityTraits.Color = ColorIndex;
 
        foreach (DBObject obj in _created)
        {
          Entity ent = obj as Entity;
          if (ent != null)
          {
            try
            {
              ent.WorldDraw(draw);
            }
            catch
            {}
          }
        }
 
        if (sol != null)
        {
          try
          {
            sol.WorldDraw(draw);
          }
          catch
          { }
        }
 
        if (_vertices.Count > 0)
        {
          Point3d lastPt = _vertices[_vertices.Count - 1];
 
          // Create a cursor sphere
 
          using (Solid3d cursor = new Solid3d())
          {
            try
            {
              cursor.CreateSphere(_profRad);
 
              if (cursor != null)
              {
                cursor.TransformBy(
                  Matrix3d.Displacement(lastPt - Point3d.Origin)
                );
 
                // Draw the cursor
 
                draw.SubEntityTraits.Color = ColorIndex;
 
                cursor.WorldDraw(draw);
              }
            }
            catch { }
          }
        }
 
        if (sol != null)
        {
          sol.Dispose();
        }
      }
 
      draw.SubEntityTraits.Color = origCol;
 
      return true;
    }
 
    public void AddSolidOrPath()
    {
      Solid3d sol = null;
      try
      {
        GenerateTube(_profRad, _vertices, out sol);
      }
      catch
      {
        if (sol != null)
        {
          sol.Dispose();
          sol = null;
        }
      }
 
      if (_created.Count > 0 || sol != null)
      {
        if (sol != null)
        {
          _created.Add(sol);
        }
 
        BlockTableRecord btr =
          (BlockTableRecord)_tr.GetObject(
            _doc.Database.CurrentSpaceId,
            OpenMode.ForWrite
          );
 
        foreach (DBObject obj in _created)
        {
          Entity ent = obj as Entity;
          if (ent != null)
          {
            //ent.ColorIndex = finalSolColor;
 
            btr.AppendEntity(ent);
            _tr.AddNewlyCreatedDBObject(ent, true);
          }
        }
        _created.Clear();
      }
 
      Cleanup();
 
      _vertices.Clear();
    }
  }
 
  public class KinectSegmentedSolidCommands
  {
    [CommandMethod("ADNPLUGINS", "KINEXT2", CommandFlags.Modal)]
    public void ImportFromKinect()
    {
      Document doc =
        Autodesk.AutoCAD.ApplicationServices.
          Application.DocumentManager.MdiActiveDocument;
      Editor ed = doc.Editor;
 
      Transaction tr =
        doc.TransactionManager.StartTransaction();
 
      // Pass in a default radius of 5cm and a segment length
      // of 10 times that
 
      KinectSegmentedSolidsJig kj =
        new KinectSegmentedSolidsJig(doc, tr, 0.05, 10);
      try
      {
        kj.StartSensor();
      }
      catch (System.Exception ex)
      {
        ed.WriteMessage(
          "\nUnable to start Kinect sensor: " + ex.Message
        );
        tr.Dispose();
        return;
      }
 
      PromptResult pr = ed.Drag(kj);
 
      if (pr.Status != PromptStatus.OK && !kj.Finished)
      {
        kj.StopSensor();
        kj.Cleanup();
        tr.Dispose();
        return;
      }
 
      // Generate a final point cloud with color before stopping
      // the sensor
 
      kj.UpdatePointCloud();
      kj.StopSensor();
 
      kj.AddSolidOrPath();
      tr.Commit();
 
      // Manually dispose to avoid scoping issues with
      // other variables
 
      tr.Dispose();
 
      kj.WriteAndImportPointCloud(doc, kj.Vectors);
    }
  }
}
- See more at: http://through-the-interface.typepad.com/through_the_interface/2011/11/adding-speech-recognition-to-autocad-via-kinect.html#sthash.XpWn3Hv3.dpuf