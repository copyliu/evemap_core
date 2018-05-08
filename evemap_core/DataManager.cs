using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using evemap_core.GraphicModel;
using evemap_core.Model;
using SolarSystem = evemap_core.Model.SolarSystem;

namespace evemap_core
{
    public class DataManager
    {
        private string conn;

        private string sovDate;
        private long time;

        public Dictionary<long, Model.Alliance> alliances=new Dictionary<long, Model.Alliance>();
        private Dictionary<int, GraphicModel.SolarSystem> systems=new Dictionary<int, GraphicModel.SolarSystem>();
        public List<GraphicModel.SolarSystem> systemsSov=new List<GraphicModel.SolarSystem>();
        Dictionary<int,List<GraphicModel.SolarSystem>> jumpsTable=new Dictionary<int, List<GraphicModel.SolarSystem>>();

        private HashSet<Color> colorTable=new HashSet<Color>();

        public Bitmap outputImage =new Bitmap(MapConstants.HORIZONTAL_SIZE,MapConstants.VERTICAL_SIZE,PixelFormat.Format32bppPArgb);
        public Graphics graphics;

        public DataManager(string dateString)
        {
            this.time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            graphics=Graphics.FromImage(outputImage);
           graphics.FillRectangle(Brushes.Black, 0,0,MapConstants.HORIZONTAL_SIZE,MapConstants.VERTICAL_SIZE);
            
        }

        public void run()
        {
            resolveDBInformation();

        }

        private void resolveDBInformation()
        {
            //            imagePanel = new JPanel(); //TODO:GUI

//            graphicsManager.setColor(new Color(0, 0, 0x0));
//            graphicsManager.fillRect(0, 0, MapConstants.HORIZONTAL_SIZE, MapConstants.VERTICAL_SIZE);
            alliances =DAL.GetAlliances().ToDictionary(p => p.id, p => p);
            var solarSystems = DAL.GetSystems().Select(p=>new GraphicModel.SolarSystem(p));
            systems = solarSystems.ToDictionary(p => p.systemid, p => p);
            var systemJumps = DAL.GetSystemJumps();
            HashSet<GraphicModel.Jump> jumps=new HashSet<Jump>(systemJumps.Select(p => new GraphicModel.Jump(systems[p.fromsystem],
                systems[p.tosystem])).Distinct());
            foreach (var systemJump in systemJumps)
            {
                if (!jumpsTable.ContainsKey(systemJump.fromsystem))
                {
                    jumpsTable.Add(systemJump.fromsystem,new List<GraphicModel.SolarSystem>());
                }

                jumpsTable[systemJump.fromsystem].Add(systems[systemJump.tosystem]);

            }

            var oldColors = DAL.GetOldSov();

            systemsSov = solarSystems.Where(p => p.allianceid > 0).ToList();

            var ssorig = systemsSov.ToList();

            foreach (var solarSystem in ssorig)
            {
                manageInfluence(solarSystem);
            }
            addNPCAlliances(systems);
            
            InfluenceCalculator influenceMap = new InfluenceCalculator(this, sovDate, jumps);
            influenceMap.oldColors = oldColors;
            influenceMap.run();
//
//            if (oldColors != null)
//                influenceMap.setOldSysSovs(oldColors);
//            //			System.out.println(Runtime.getRuntime().freeMemory()/1048576 + "MB");
//            new Thread(influenceMap, "Influence Generator Thread").start();


        }

        private void addNPCAlliances(Dictionary<int, GraphicModel.SolarSystem> solarSystems)
        {
            var npcs=DAL.GetNpcInfs();
            Color npcclolr = Color.FromArgb(0, 0, 0, 0);
           
            foreach (var npcInf in npcs)
            {
                var alliance = new Alliance()
                {
                    id = npcInf.npcid,
                    name = npcInf.name,
                    colorString = "000000",
                    isNPC = true
                };
                this.alliances[npcInf.npcid] = alliance;
                addInfluence(solarSystems[npcInf.systemid],npcInf.inf, alliance, 4,new List<GraphicModel.SolarSystem>());
            }
        }

        private void manageInfluence(GraphicModel.SolarSystem ss)
        {
            double influence = 10.0;
            int level = 2;
            if (ss.hasstatsion)
            {
                influence *= 6;
                level = 1;
            }

            switch (ss.sovlevel)
            {
                case 0: influence *= 0.5;
                    break;
                case 2: influence *= 1.1;
                    break;
                case 3:
                    influence *= 1.2;
                    break;
                case 4:
                    influence *= 1.4;
                    break;
                case 5:
                    influence *= 1.6;
                    break;

            }

            Alliance al;
            if (alliances.ContainsKey(ss.allianceid))
            {
                al = alliances[ss.allianceid];
            }
            else

            {
                al = null;
            }
            addInfluence(ss,influence,al,level,new List<GraphicModel.SolarSystem>());


         
        }

        private void addInfluence(GraphicModel.SolarSystem ss, double value, Alliance al, int level, List<GraphicModel.SolarSystem> set)
        {
            ss.addInfluence(al,value);
            if (!systemsSov.AsParallel().Any(p=>p.systemid==ss.systemid))
            {
                systemsSov.Add(ss);
            }

            if (level >= 4) return;
            var arr = jumpsTable[ss.systemid];
            foreach (var s in arr)
            {
                if (set.Contains(s))continue;
                set.Add(s);
                addInfluence(s,value*0.3,al,level+1,set);
            }

        }
    }
}