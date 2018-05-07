using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using evemap_core.Model;
using MySql.Data.MySqlClient;

namespace evemap_core
{
    public  class DAL
    {
        public static string connString;
        public static List<Model.Alliance> GetAlliances()
        {
            var results = new List<Model.Alliance>();
            using (MySqlConnection conn=new MySqlConnection(connString))
            {
                conn.Open();
                MySqlCommand cmd=new MySqlCommand("SELECT id,name,color FROM evealliances;",conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new Alliance()
                        {
                            id=Convert.ToInt64( reader[0]),
                            name=reader[1]+"",
                            colorString = reader[2]+"",
                            isNPC = false
                        });
                    }
                }
            }

            return results;
        }
        public static List<Model.SolarSystem> GetSystems()
        {
            var results = new List<Model.SolarSystem>();
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(@"SELECT solarSystemID,
allianceID,
stantion,
constellationID,
regionID,
sovereigntyLevel,
x,z FROM mapsolarsystems;", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new SolarSystem()
                        {
                            systemid = Convert.ToInt32(reader[0]),
                            allianceid = Convert.ToInt64(reader[1]),
                            stations = Convert.ToInt32(reader[2]),
                            constellationid = Convert.ToInt32(reader[3]),
                            regionid = Convert.ToInt32(reader[4]),
                            sovlevel = Convert.ToInt32(reader[5]),
                            x = Convert.ToDouble(reader[6]),
                            z = Convert.ToDouble(reader[7]),

                        });
                    }
                }
            }

            return results;
        }

        public static List<Model.SystemJump> GetSystemJumps()
        {
            var results = new List<Model.SystemJump>();
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(@"SELECT fromSolarSystemID,
toSolarSystemID FROM mapsolarsystemjumps", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new SystemJump()
                        {
                           fromsystem = Convert.ToInt32(reader[0]),
                            tosystem = Convert.ToInt32(reader[1])

                        });
                    }
                }
            }

            return results;
        }
        public static List<Model.NPCInf> GetNpcInfs()
        {
            var results = new List<Model.NPCInf>();
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(@"SELECT id,
name,systemID,influence FROM npcalliances", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new NPCInf()
                        {
                           npcid = Convert.ToInt32(reader[0]),
                            name = reader[1]+"",

                            systemid = Convert.ToInt32(reader[2]),
                            inf = Convert.ToDouble(reader[3]),

                        });
                    }
                }
            }

            return results;
        }

        public static long[,] GetOldSov()
        {
            var results = new long[MapConstants.HORIZONTAL_SIZE + 1, MapConstants.VERTICAL_SIZE + 1];
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(@"SELECT alliance,x,y FROM sov_change_information", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int x = (int) reader[1] ;

//                        int x = Convert.ToInt32(reader[1]);
                        int y = (int) reader[2];
                        long allid = (long ) reader[0];
                        results[x, y] = allid;
                    }
                }
            }

            return results;
        }

        public static void saveColor(Alliance best)
        {
            //TODO
        }

        public static void saveOldAlliance(SovData sovData)
        {
            //TODO
        }
    }
}
