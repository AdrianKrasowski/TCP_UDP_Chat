using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sLanCS
{
    interface sLanCS
    {
        void Show();
    }

    class Client : sLanCS
    {
        public void Show()
        {
            Form3 f3 = new Form3();
            f3.Show();
        }
    }

    class Server : sLanCS
    {
        public void Show()
        {
            Form2 f2 = new Form2();
            f2.Show();
        }
    }
    
    abstract class Factory
    {
        public abstract sLanCS FactoryMethod(string type);
    }

    class sLanCSFactory : Factory
    {
        public override sLanCS FactoryMethod(string type)
        {
            switch (type)
            {
                case "Client":
                    return new Client();
                case "Server":
                    return new Server();
                default:
                    return null;
            }
        }
    }
}
