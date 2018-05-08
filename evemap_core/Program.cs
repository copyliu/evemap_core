using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace evemap_core
{
    class Program
    {
        private static string connectionstring = "Server=127.0.0.1;Database=evemap;Uid=root;Pwd=;SslMode=none";


        private static bool isBatch = false;

        private static string date = "";
        static void Main(string[] args)
        {
            parseVariables(args);
            string dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            if (date != "")
            {
                dateString = date;
            }

//            MapConstants.THREADPOOL_SIZE = 1;
            DAL.connString = connectionstring;
//            Connection[] db;
            try
            {
                //             Initialize SQL connections, One for each thread. Connection is not
                //             thread safe.
//                DriverManager.registerDriver(new com.mysql.jdbc.Driver());
//                db = new Connection[(MapConstants.THREADPOOL_SIZE + 1)];
//                for (int con = 0; (con
//                                   < (MapConstants.THREADPOOL_SIZE + 1)); con++)
//                {
//                    db[con] = DriverManager.getConnection(url, root, pw);
//                }

                DataManager data = new DataManager(dateString);
                if (!isBatch)
                {
                    //                    new StarMapGeneratorFrame(data);

                    var t = new Thread(() => {
                        var F = new System.Windows.Forms.Form();
                        var p = new System.Windows.Forms.PictureBox();
                        p.Image = data.outputImage;
                        
                        
//                        p.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                        p.Margin=Padding.Empty;
                        p.Left = 0;
                        p.Top = 0;
                        p.Width = (int) (MapConstants.HORIZONTAL_SIZE / 2);
                        p.Height = (int)(MapConstants.VERTICAL_SIZE / 2);
//                        p.AutoSize = true;
                        p.SizeMode = PictureBoxSizeMode.StretchImage;
                        F.Controls.Add(p);
                        Timer timer=new Timer();
                        timer.Interval = 100;
                        timer.Tick += (sender, eventArgs) => p.Refresh();
                        timer.Start();
                        F.ShowDialog();
                    });
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    

                }
                Thread t2=new Thread(data.run);
                t2.Start();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private static void parseVariables(string[] args)
        {
            for (int i = 0; (i < args.Length); i++)
            {
                string arg = args[i];
                if (arg.ToLower()=="-conn")
                {
                    connectionstring = args[++i];
                }
               
                else if (arg.ToLower() == ("-isBatch"))
                {
                    String inp = args[++i];
                    isBatch = (inp.ToLower() == ("t") || inp.ToLower() == ("true"));
                }
                else if (arg.ToLower() == ("-date"))
                {
                    date = args[++i];
                }
                else if (arg.ToLower() == ("-thread"))
                {
                    if (Int32.TryParse(args[++i], out var count))
                    {
                        MapConstants.THREADPOOL_SIZE = count;
                    }
                   
                }
                else if (arg.ToLower() == ("-help"))
                {
                    Console.WriteLine ("Valid Arguments:");
                    Console.WriteLine("[-conn : The Connection String of DB server]");
                    Console.WriteLine("[-isBatch : [t f true false] Defaults to false, when true no UI shows]");
                    Console.WriteLine("[-date : map date");
                    Console.WriteLine("[-help : Display the set of acceptable commands]");
                }
                else
                {
                    Console.WriteLine(("Cannot parse "
                                        + (arg + ", Unknown command.")));
                   
                }

            }

        }
    }
}
