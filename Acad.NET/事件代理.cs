using System;
 
namespace Event
{
    //����1�������¼�������
    public class MyEventArgs : EventArgs
    {
        private string message;
       
        /// <summary>
        /// ��Ϣ
        /// </summary>
        public string Message
        {
            get { return message; }
        }
 
        //���캯��
        public MyEventArgs(string message)
        {
            this.message = message;
        }
    }
 
    //����2������delegate����
    public delegate void MyEventHandler(object sender, MyEventArgs e);
   
    /// <summary>
    /// �¼�������
    /// </summary>
    class Publisher
    {
        //����3�������¼�����
        public event MyEventHandler MyEvent;
 
        public void RaiseEvent()
        {
            MyEventArgs e = new MyEventArgs("Hello,World!");
 
            //����4�������¼�
            if (MyEvent != null)
            {
                MyEvent(this, e);
            }
        }
 
        #region
 
        //�����RaiseEvent����������������Ĵ����滻��
        //�Ա������ܹ��̳�/Override�����¼��ķ�����������ָOnMyEvent����
 
        //public void RaiseEvent()
        //{
        //    MyEventArgs e = new MyEventArgs("Hello,World!");
        //    OnMyEvent(e);
        //}
 
        //protected void OnMyEvent(MyEventArgs e)
        //{
        //    if (MyEvent != null)
        //    {
        //        MyEvent(this, e);
        //    }
        //}
 
        #endregion
 
    }
 
    /// <summary>
    /// �¼�������
    /// </summary>
    class Subscriber
    {
        private string id;
        public Subscriber(string id, Publisher publisher)
        {
            this.id = id;
 
            //����6����+=����������¼����¼������У�-=�������ܹ����¼��Ӷ�����ɾ������
            publisher.MyEvent += new MyEventHandler(publisher_MyEvent);
        }
 
        //����5�������¼�������
        void publisher_MyEvent(object sender, MyEventArgs e)
        {
            Console.WriteLine(id + " received this message: {0}", e.Message);
        }
    }
 
    class Program
    {
        static void Main(string[] args)
        {
            Publisher pub = new Publisher();
            Subscriber sub1 = new Subscriber("sub1", pub);
            Subscriber sub2 = new Subscriber("sub2", pub);
 
            //����7�����ʵ��ĵط������¼��������������¼���
            pub.RaiseEvent();
 
            //Keep the console window open
            Console.WriteLine("Press Enter to close this window.");
            Console.ReadLine();
        }
    }
}