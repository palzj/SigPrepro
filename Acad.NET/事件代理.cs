using System;
 
namespace Event
{
    //步骤1，定义事件参数类
    public class MyEventArgs : EventArgs
    {
        private string message;
       
        /// <summary>
        /// 信息
        /// </summary>
        public string Message
        {
            get { return message; }
        }
 
        //构造函数
        public MyEventArgs(string message)
        {
            this.message = message;
        }
    }
 
    //步骤2，定义delegate对象
    public delegate void MyEventHandler(object sender, MyEventArgs e);
   
    /// <summary>
    /// 事件发布者
    /// </summary>
    class Publisher
    {
        //步骤3，定义事件对象
        public event MyEventHandler MyEvent;
 
        public void RaiseEvent()
        {
            MyEventArgs e = new MyEventArgs("Hello,World!");
 
            //步骤4，触发事件
            if (MyEvent != null)
            {
                MyEvent(this, e);
            }
        }
 
        #region
 
        //上面的RaiseEvent方法还可以用下面的代码替换，
        //以便子类能够继承/Override触发事件的方法，在这里指OnMyEvent方法
 
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
    /// 事件订阅者
    /// </summary>
    class Subscriber
    {
        private string id;
        public Subscriber(string id, Publisher publisher)
        {
            this.id = id;
 
            //步骤6．用+=操作符添加事件到事件队列中（-=操作符能够将事件从队列中删除）。
            publisher.MyEvent += new MyEventHandler(publisher_MyEvent);
        }
 
        //步骤5，定义事件处理方法
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
 
            //步骤7．在适当的地方调用事件触发方法触发事件。
            pub.RaiseEvent();
 
            //Keep the console window open
            Console.WriteLine("Press Enter to close this window.");
            Console.ReadLine();
        }
    }
}