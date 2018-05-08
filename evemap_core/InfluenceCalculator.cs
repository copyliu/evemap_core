using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using evemap_core.GraphicModel;
using evemap_core.Model;
using SolarSystem = evemap_core.GraphicModel.SolarSystem;

namespace evemap_core
{
    public class SovData
    {
        public int x, y;
        public long al;

    }

    public class CalculateRow
    {
        private static double INSENSITIVITY = 500;
        private static double VALIDINF = 0.023;

        Dictionary<long, double> totalInf = new Dictionary<long, double>();

        private int x_from;
        private int x_to;
        private int currentRow;
        private double max = 0;

        private InfluenceCalculator map;
        public Dictionary<long, Alliance> alliances;
        public long[,] oldColors;

        private double[] prevInf;
        private bool[] curBorder;
        private Alliance[] curRow;
        private Alliance[] prevRow;

        private Alliance[,] sovMap;
        List<SovData> group;

        public CalculateRow(int xFrom, int xTo, InfluenceCalculator map, int quanta)
        {
            this.x_from = xFrom;
            this.x_to = xTo;
            this.map = map;
            prevInf = new double[quanta];
            curBorder = new bool[quanta];
            curRow = new Alliance[quanta];
            prevRow = new Alliance[quanta];
            sovMap = new Alliance[MapConstants.HORIZONTAL_SIZE / MapConstants.SAMPLE_RATE,
                MapConstants.VERTICAL_SIZE / MapConstants.SAMPLE_RATE];
        }



        public void run()
        {
            curRow = new Alliance[MapConstants.HORIZONTAL_SIZE / MapConstants.THREADPOOL_SIZE];
            var sSov = this.map.dataManager.systemsSov.ToList();
            while (true)
            {
                currentRow++;
                Console.WriteLine(currentRow);
                for (int x = x_from; x < x_to; x++)
                {
                    totalInf.Clear();
                    getTotalInfluenceforPoint(x, currentRow, sSov, totalInf);

                    max = 0.0;
                    var bestalliid = getAllianceWithHighestInfluence(totalInf, false);
                    var best = bestalliid.HasValue ? alliances[bestalliid.Value] : null;

                    int q = x - x_from;
                    if (best != null)
                    {
                        if (best.colorString == null)
                        {
                            if (best.isNPC)
                            {
                                best.colorString = "000000";
                            }
                            else
                            {
                                //TODO 新顏色
                            }

                            DAL.saveColor(best);
                        }


                        best.translate(x, currentRow);
                        best.incrementCount();
                    }

                    if (best != null && (x % MapConstants.SAMPLE_RATE == 0) &&
                        (currentRow % MapConstants.SAMPLE_RATE) == 0)
                    {
                        sovMap[x / MapConstants.SAMPLE_RATE, currentRow / MapConstants.SAMPLE_RATE] =
                            best.isNPC ? null : best;
                    }

                    curRow[q] = best;

                    if (currentRow > 0)
                    {
                        Alliance prevAlliance = prevRow[q];

                        if (prevAlliance != null && !prevAlliance.isNPC)
                        {
                            saveAllianceAtPosition(x, currentRow, prevAlliance.id);
                            drawSolidWithBorder(q, x, currentRow, best, curBorder, prevRow, prevInf);

                            if (oldColors[x, currentRow] == 0 || prevAlliance.id != oldColors[x, currentRow])
                                if (oldColors[x, currentRow] != 0)
                                    drawLined(q, x, currentRow, alliances[oldColors[x, currentRow]].color, prevInf);

                        }
                    }

                    prevInf[q] = max;
                    curBorder[q] = currentRow == 0 || prevRow[q] == null && best != null ||
                                   prevRow[q] != null && best == null || prevRow[q] != null && prevRow[q] != best;



                }

                Array.Copy(curRow, prevRow, curRow.Length);
                if (currentRow == MapConstants.VERTICAL_SIZE - 1)
                {
                    break;
                }
            }

            flush();
            Array.Copy(prevInf, 0, map.previnf, x_from, x_to - x_from - 1);
            Array.Copy(curRow, 0, map.prevRow, x_from, x_to - x_from);
            lock (map.sovMap)
            {
                for (int i = 0; i < sovMap.Rank; i++)
                {
                    for (int j = 0; j < sovMap.GetLength(i); j++)
                    {
                        if (sovMap[i, j] != null)
                        {
                            map.sovMap[i, j] = sovMap[i, j];
                        }
                    }
                }
            }
        }


        private void flush()
        {

        }

        private void drawLined(int q, int x, int y, Color co, double[] cprevInf)
        {
            int alpha = Math.Min(190, (int) (Math.Log(Math.Log(cprevInf[q] + 1.0) + 1.0) * 700));
            Color c = Color.FromArgb(alpha, co);
            drawColorLined(x, y, c);
        }

        private void drawColorLined(int x, int y, Color c)
        {
            int slant = 5;
            if ((((y % slant) + x) % slant) == 0)
            {
//                map.dataManager.outputImage.SetPixel(x,y,c);

                lock (map.dataManager.graphics)
                { map.dataManager.graphics.FillRectangle(new SolidBrush(c), x, y, 1, 1);}
            }

        }

        private void drawSolidWithBorder(int q, int x, int y, Alliance best, bool[] cborder, Alliance[] cprevRow,
            double[] cprevInf)
        {
            var border = cborder[q] || cprevRow[q] == null && best != null || cprevRow[q] != null && best == null ||
                         best != cprevRow[q] || q > 0 && cprevRow[q] != cprevRow[q - 1] ||
                         (cprevRow.Length > q + 1 && cprevRow[q] != cprevRow[q + 1]);

            int alpha = Math.Min(190, (int) (Math.Log(Math.Log(cprevInf[q] + 1.0) + 1.0) * 700));

            Color c = Color.FromArgb(border ? Math.Max(0x48, alpha) : alpha, cprevRow[q].color);
//            map.dataManager.outputImage.SetPixel(x, y, c);
            lock (map.dataManager.graphics)
            { map.dataManager.graphics.FillRectangle(new SolidBrush(c), x, y-1, 1, 1);}
          
            




        }

        private void saveAllianceAtPosition(int x, int y, long c)
        {

            DAL.saveOldAlliance(new SovData()
            {
                x = x,
                y = y,
                al = c
            });

        }

        private long? getAllianceWithHighestInfluence(Dictionary<long, double> totalInfluence, bool isOld)
        {
            long? best = null;
            double d;
            double priMax = max;
            
            foreach (var al in totalInfluence.Keys)

            {
                d = totalInfluence[al];
                if (d > priMax)
                {
                    priMax = d;
                    best = al;
                }
            }

            if (priMax < VALIDINF) best = null;
            if (isOld)
            {
            }
            else
            {
                max = priMax;
            }

            return best;
        }

        private void getTotalInfluenceforPoint(int x, int y, List<SolarSystem> sSov,
            Dictionary<long, double> totalInfluence)
        {
            int dx, dy, len2;
            double d;


//            var ssfilter = sSov.AsParallel().Where(p => 
//                    
//                    p.x<(x+400) && p.x>(x-400) && p.y<(y+400) && p.y>(y-400)
//                    )
////                .Where(p => (Math.Pow(x - p.x, 2) + Math.Pow(y - p.y, 2)) < 160000)
//                .Where(p => p.alliances.Count > 0);

            var xmin = (x < 400 ? 0 : x - 400)/MapConstants.CALC_SAMPLE;
            var xmax = (x+400 > MapConstants.HORIZONTAL_SIZE  ? MapConstants.HORIZONTAL_SIZE : x + 400)/ MapConstants.CALC_SAMPLE;
            var ymin =(y < 400 ? 0 : y - 400) / MapConstants.CALC_SAMPLE;
            var ymax = (y+400 > MapConstants.VERTICAL_SIZE ? MapConstants.VERTICAL_SIZE : y + 400) / MapConstants.CALC_SAMPLE;

            for (int xx = xmin; xx <= xmax; xx++)
            {
                for (int yy = ymin; yy <= ymax; yy++)
                {
                    bool has_point_in_c = false;
                    bool all_point_in_c = true;
                    //TOP LEFT
                    dx = x - xx * MapConstants.CALC_SAMPLE;
                    dy = y - yy * MapConstants.CALC_SAMPLE;
                    len2 = dx * dx + dy * dy;
                    if (dx > 400 || dx < -400|| dy > 400 || dy < -400 || len2 > 160000)
                        all_point_in_c = false;
                    else
                        has_point_in_c = true;

                    //TOP RIGHT
                    dx = x - (xx + 1) * MapConstants.CALC_SAMPLE;
                    dy = y - yy * MapConstants.CALC_SAMPLE;
                    len2 = dx * dx + dy * dy;
                    if (dx > 400 || dx < -400 || dy > 400 || dy < -400 || len2 > 160000)
                        all_point_in_c = false;
                    else
                        has_point_in_c = true;

                    //BTN LEFT
                    dx = x - xx * MapConstants.CALC_SAMPLE;
                    dy = y - (yy + 1) * MapConstants.CALC_SAMPLE;
                    len2 = dx * dx + dy * dy;
                    if (dx > 400 || dx < -400 || dy > 400 || dy < -400 || len2 > 160000)
                        all_point_in_c = false;
                    else
                        has_point_in_c = true;

                    //BTN RIGHT
                    dx = x - (xx + 1) * MapConstants.CALC_SAMPLE;
                    dy = y - (yy + 1) * MapConstants.CALC_SAMPLE;
                    len2 = dx * dx + dy * dy;
                    if (dx > 400 || dx < -400 || dy > 400 || dy < -400 || len2 > 160000)
                        all_point_in_c = false;
                    else
                        has_point_in_c = true;

                    if (!has_point_in_c)
                    {
                        continue; //no points in circle
                       
                    }



                    if (this.map.systemMap[xx, yy] != null)
                    {
                        foreach (var ss in this.map.systemMap[xx, yy])
                        {
                            if (all_point_in_c && false)//TODO 似乎有bug
                            {
                            }
                            else
                            {
                                dx = x - ss.x;

                                if (dx > 400 || dx < -400)
                                {
                                    continue;
                                }

                                dy = y - ss.y;

                                if (dy > 400 || dy < -400)
                                {
                                    continue;
                                }

                                len2 = dx * dx + dy * dy;
                                if (len2 > 160000)
                                {
                                    continue;
                                }
                            }

                            for (int loc = 0; loc < ss.alliances.Count; loc++)
                            {
                                d = 0;
                                totalInfluence.TryGetValue(ss.alliances[loc].id, out d);
                                totalInfluence[ss.alliances[loc].id] = ss.influences[loc] / (INSENSITIVITY + len2) + d;
                            }
                        }
                    }
                }
            }
//            var ssfilter = new List<SolarSystem>();
//            var xkey = this.map.systemMap.Keys.Where(p=>p>x-400 && p<x+400).ToList();
//
//            foreach (var x1 in xkey)
//            {
//           
//                var ykey = this.map.systemMap[x1].Keys.Where(p => p > y - 400 && p < y + 400).ToList();
//                foreach (var y2 in ykey)
//                {
//                    ssfilter.AddRange(this.map.systemMap[x1][y2]);
//                }
//
//            }
//
//           
////
////            var ssfilter1 = this.map.systemMap[x];
////            if (ssfilter1 == null)
////            {
////                return;}
////    var ssfilter = ssfilter1.AsParallel().Where(p => 
////            
////            p.y<(y+400) && p.y>(y-400)
////            )
//////                .Where(p => (Math.Pow(x - p.x, 2) + Math.Pow(y - p.y, 2)) < 160000)
////        .ToList();
//            foreach (var ss in ssfilter)
//            {
//                dx = x - ss.x;
//
//                //                                if (dx > 400 || dx < -400)
//                //                                    continue;
//                dy = y - ss.y;
//
//                //                                if (dy > 400 || dy < -400)
//                //                                    continue;
//                len2 = dx * dx + dy * dy;
//
//               
//
//
//                for (int loc = 0; loc < ss.alliances.Count; loc++)
//                {
//                    d = 0;
//                    totalInfluence.TryGetValue(ss.alliances[loc].id, out d);
//                    totalInfluence[ss.alliances[loc].id] = ss.influences[loc] / (INSENSITIVITY + len2) + d;
//                }
//            }
//          



        }
    }


    public class InfluenceCalculator
    {
        public DataManager dataManager;
        private string sovDate;
        private HashSet<Jump> jumps;
        public long[,] oldColors=null;

        public double[] previnf=new double[MapConstants.HORIZONTAL_SIZE+1];
        public Alliance[] prevRow=new Alliance[MapConstants.HORIZONTAL_SIZE];
        public Alliance[,] sovMap = new Alliance[MapConstants.HORIZONTAL_SIZE / MapConstants.SAMPLE_RATE,MapConstants.VERTICAL_SIZE / MapConstants.SAMPLE_RATE];


        //        public List<SolarSystem>[] systemMap=new List<SolarSystem>[MapConstants.HORIZONTAL_SIZE];
        public List<SolarSystem>[,] systemMap=new List<SolarSystem>[MapConstants.HORIZONTAL_SIZE / MapConstants.CALC_SAMPLE + 1, MapConstants.VERTICAL_SIZE / MapConstants.CALC_SAMPLE + 1];
//       public  Dictionary<int,Dictionary<int,List<SolarSystem>>> systemMap=new Dictionary<int, Dictionary<int, List<SolarSystem>>>();
//        public Dictionary<int,HashSet<int>> xycache=new Dictionary<int, HashSet<int>>();
        public InfluenceCalculator(DataManager dataManager, string sovDate, HashSet<Jump> jumps)
        {
            this.dataManager = dataManager;
            this.sovDate = sovDate;
            this.jumps = jumps;
        }

        public void run()
        {
//            systemMap.Initialize();
            var tmp = dataManager.systemsSov.ToList();
            int i0 = 0;
            foreach (var ss in tmp)
            {
                Console.WriteLine(i0);
                i0++;
                var sx = ss.x / MapConstants.CALC_SAMPLE;
                var sy = ss.y / MapConstants.CALC_SAMPLE;
                if (systemMap[sx, sy] == null)
                {
                    systemMap[sx,sy]=new List<SolarSystem>();
                }
                systemMap[sx,sy].Add(ss);

                
               
            }


           
            Parallel.ForEach(Enumerable.Range(0, MapConstants.THREADPOOL_SIZE),new ParallelOptions(){MaxDegreeOfParallelism = MapConstants.THREADPOOL_SIZE }, i =>
            {
                int quanta = MapConstants.HORIZONTAL_SIZE / MapConstants.THREADPOOL_SIZE * (i + 1) -
                             MapConstants.HORIZONTAL_SIZE / MapConstants.THREADPOOL_SIZE * i;
                var worker = new CalculateRow(
                    MapConstants.HORIZONTAL_SIZE / MapConstants.THREADPOOL_SIZE * i,
                    MapConstants.HORIZONTAL_SIZE / MapConstants.THREADPOOL_SIZE * (i + 1),
                    this, quanta
                );
                worker.alliances = this.dataManager.alliances;
                worker.oldColors = this.oldColors;
                worker.run();
            });
          


        }

       
    }
}
