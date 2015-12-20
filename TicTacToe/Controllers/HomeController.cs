using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;

namespace TicTacToe.Controllers
{
    public enum MoveType
    {
        X=1,
        O=2
    }

    public class Veri
    {
        public List<string> Xmoves { get; set; }
        public List<string> Omoves { get; set; }
        public int MoveType { get; set; }
        public List<string> Player { get; set; }
    }
    public class HomeController : Controller
    {
        //
        // GET: /Home/        

        public ActionResult Index()
        {

            //Get Cookie Player Name
            if (System.Web.HttpContext.Current.Request.Cookies["TicTacToeNick"]!=null && System.Web.HttpContext.Current.Request.Cookies["TicTacToeNick"]["TicTacToePlayer"] != null)
            {
                if (System.Web.HttpContext.Current.Cache["PlayerList"] != null)
                {
                    string playerName = System.Web.HttpContext.Current.Request.Cookies["TicTacToeNick"]["TicTacToePlayer"];
                    string playerID = System.Web.HttpContext.Current.Request.Cookies["TicTacToeNick"]["TicTacToePlayerID"];

                    if (((List<string>)System.Web.HttpContext.Current.Cache["PlayerList"]).Count == 1 && playerName != ((List<string>)System.Web.HttpContext.Current.Cache["PlayerList"]).FirstOrDefault())
                    {
                        HttpCookie myCookie = new HttpCookie("TicTacToeNick");
                        myCookie.Expires = DateTime.Now.AddDays(-1d);
                        System.Web.HttpContext.Current.Response.Cookies.Add(myCookie);
                    }
                    else
                    {
                        ViewBag.PlayerCookie = playerName.ToUpper();
                        ViewBag.realPlayerCookie = playerName;
                        ViewBag.PlayerID = playerID;
                    }
                }
                else
                {
                    HttpCookie myCookie = new HttpCookie("TicTacToeNick");
                    myCookie.Expires = DateTime.Now.AddDays(-1d);
                    System.Web.HttpContext.Current.Response.Cookies.Add(myCookie);
                }
            }

            Veri ver = new Veri();
            ver.Xmoves = System.Web.HttpContext.Current.Cache["Xmoves"] as List<string>;
            ver.Omoves = System.Web.HttpContext.Current.Cache["Omoves"] as List<string>;
            if(System.Web.HttpContext.Current.Cache["MoveType"]!=null)
                ver.MoveType = (int)System.Web.HttpContext.Current.Cache["MoveType"];
            if (System.Web.HttpContext.Current.Cache["PlayerList"] != null)
                ver.Player = (List<string>)System.Web.HttpContext.Current.Cache["PlayerList"];
            else
                ver.Player = new List<string>();
            return View(ver);
        }
        public static void recordCordinates(string cordinate,int moveType)
        {
            Veri ver = new Veri();
            ver.Xmoves = System.Web.HttpContext.Current.Cache["Xmoves"] as List<string>;
            ver.Omoves = System.Web.HttpContext.Current.Cache["Omoves"] as List<string>;
            if (System.Web.HttpContext.Current.Cache["MoveType"] != null)
                ver.MoveType = (int)System.Web.HttpContext.Current.Cache["MoveType"];
            if (moveType == (int)MoveType.X)
            {
                if (ver.Xmoves == null)
                {
                    List<string> Xmoves = new List<string>();
                    Xmoves.Add(cordinate);
                    System.Web.HttpContext.Current.Cache.Insert("Xmoves", Xmoves, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(50));
                    System.Web.HttpContext.Current.Cache.Insert("MoveType", moveType, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(50));
                }
                else
                {
                    ver.Xmoves.Add(cordinate);
                    System.Web.HttpContext.Current.Cache["Xmoves"] = ver.Xmoves;
                    System.Web.HttpContext.Current.Cache["MoveType"]= moveType;
                }
            }
            else if (moveType == (int)MoveType.O)
            {
                if (ver.Omoves == null)
                {
                    List<string> Omoves = new List<string>();
                    Omoves.Add(cordinate);
                    System.Web.HttpContext.Current.Cache.Insert("Omoves", Omoves, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(50));
                    System.Web.HttpContext.Current.Cache.Insert("MoveType", moveType, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(50));
                }
                else
                {
                    ver.Omoves.Add(cordinate);
                    System.Web.HttpContext.Current.Cache["Omoves"] = ver.Omoves;
                    System.Web.HttpContext.Current.Cache["MoveType"]= moveType;
                }
            }
        }

        static private List<int> Xmoves = new List<int>();
        static private List<int> Omoves = new List<int>();
        static private List<string> PlayerList = new List<string>();
        public class TicTac : Hub
        {          
            static private int[,] Cordinates = new int[,]
            {
                 {0,1,2},
                 {3,4,5},
                 {6,7,8}
            };
            static private int[,] Winners = new int[,]
            {
                 {0,1,2},
                 {3,4,5},
                 {6,7,8},
                 {0,3,6},
                 {1,4,7},
                 {2,5,8},
                 {0,4,8},
                 {2,4,6}
            };
            private int insertCordinate(string cordinate, int moveType, out bool doIT)
            {
                doIT = true;
                int index=cordinate.ToCharArray().ToList().FindIndex(c => c != '0');
                int value = int.Parse(cordinate.ToCharArray().ToList().Where(c => c != '0').FirstOrDefault().ToString()) - 1;
                int cord = Cordinates[value, index];
                if (moveType == (int)MoveType.X)
                {

                    if (Xmoves.Contains(cord) == false && Omoves.Contains(cord) == false)
                    {
                        Xmoves.Add(cord);
                        //System.Web.HttpContext.Current.Cache.Insert("Xmoves", Xmoves, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(1000));
                        recordCordinates(cordinate, moveType);
                        for (int i = 0; i < 8; i++)
                        {
                            int a = Winners[i, 0], b = Winners[i, 1], c = Winners[i, 2];
                            if (Xmoves.Contains(a) && Xmoves.Contains(b) && Xmoves.Contains(c))
                            {
                                Xmoves.Clear();
                                Omoves.Clear();
                                System.Web.HttpContext.Current.Cache.Remove("Omoves");
                                System.Web.HttpContext.Current.Cache.Remove("Xmoves");
                                System.Web.HttpContext.Current.Cache.Remove("MoveType");
//                                string playerID = System.Web.HttpContext.Current.Cache["PlayerID"].ToString();
//                                System.Web.HttpContext.Current.Cache.Remove("PlayerID");
//                                System.Web.HttpContext.Current.Cache.Remove("Player" + playerID);
                                return 1;
                            }
                        }
                    }
                    else
                    {
                        doIT = false;
                    }
                }
                else if (moveType == (int)MoveType.O)
                {
                    if (Xmoves.Contains(cord) == false && Omoves.Contains(cord) == false)
                    {
                        Omoves.Add(cord);
                        //System.Web.HttpContext.Current.Cache.Insert("Omoves", Omoves, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(1000));
                        recordCordinates(cordinate, moveType);
                        for (int i = 0; i < 8; i++)
                        {
                            int a = Winners[i, 0], b = Winners[i, 1], c = Winners[i, 2];
                            if (Omoves.Contains(a) && Omoves.Contains(b) && Omoves.Contains(c))
                            {
                                Xmoves.Clear();
                                Omoves.Clear();
                                System.Web.HttpContext.Current.Cache.Remove("Omoves");
                                System.Web.HttpContext.Current.Cache.Remove("Xmoves");
                                System.Web.HttpContext.Current.Cache.Remove("MoveType");
//                                string playerID = System.Web.HttpContext.Current.Cache["PlayerID"].ToString();
//                                System.Web.HttpContext.Current.Cache.Remove("PlayerID");
//                                System.Web.HttpContext.Current.Cache.Remove("Player" + playerID);
                                return 1;
                            }
                        }
                    }
                    else
                    {
                        doIT = false;
                    }
                }
                return 0;
            }

            public void checkMove(string cordinates, int moveType)
            {
                bool doIT;
                int winner=insertCordinate(cordinates,moveType,out doIT);
                if ((Xmoves.Count + Omoves.Count) == 9 || (Xmoves.Count + Omoves.Count) > 9)
                {
                    winner = 2;
                    Xmoves.Clear();
                    Omoves.Clear();
                    System.Web.HttpContext.Current.Cache.Remove("Omoves");
                    System.Web.HttpContext.Current.Cache.Remove("Xmoves");
                    System.Web.HttpContext.Current.Cache.Remove("MoveType");
//                    string playerID = System.Web.HttpContext.Current.Cache["PlayerID"].ToString();
//                    System.Web.HttpContext.Current.Cache.Remove("PlayerID");
//                    System.Web.HttpContext.Current.Cache.Remove("Player" + playerID);
                }
                Clients.All.insertMove(cordinates,winner,doIT,moveType);
            }

            public bool saveCookie(string player, int playerID)
            {
                try
                {
                    if(!PlayerList.Contains(player+"æ"+playerID.ToString()))
                    {
                        PlayerList.Add(player + "æ" + playerID.ToString());
                    }
                    System.Web.HttpContext.Current.Cache.Insert("PlayerList", PlayerList, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(50));

                    HttpCookie myCookie = new HttpCookie("TicTacToeNick");
                    myCookie["TicTacToePlayer"] = player;
                    myCookie["TicTacToePlayerID"] = playerID.ToString();
                    myCookie.Expires = DateTime.Now.AddMinutes(50);
                    System.Web.HttpContext.Current.Response.Cookies.Add(myCookie);

                    System.Web.HttpContext.Current.Cache.Insert("Player" + playerID.ToString(), player, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(50));
                    System.Web.HttpContext.Current.Cache.Insert("PlayerID", playerID.ToString(), null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(50));

                    Clients.All.setNick(playerID, player);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public bool delCookie(int playerID)
            {
                try
                {
                    if (System.Web.HttpContext.Current.Request.Cookies["TicTacToeNick"] != null)
                    {
                        HttpCookie myCookie = new HttpCookie("TicTacToeNick");
                        myCookie.Expires = DateTime.Now.AddDays(-1d);
                        System.Web.HttpContext.Current.Response.Cookies.Add(myCookie);
                    }

                    System.Web.HttpContext.Current.Cache.Remove("Player" + playerID.ToString());

                    System.Web.HttpContext.Current.Cache.Remove("Omoves");
                    System.Web.HttpContext.Current.Cache.Remove("Xmoves");
                    System.Web.HttpContext.Current.Cache.Remove("MoveType");
                    if (System.Web.HttpContext.Current.Cache["PlayerID"] != null)
                    {
                        string pID = System.Web.HttpContext.Current.Cache["PlayerID"].ToString();
                        System.Web.HttpContext.Current.Cache.Remove("PlayerID");
                        System.Web.HttpContext.Current.Cache.Remove("Player" + pID);
                    }
                    else
                    {
                        System.Web.HttpContext.Current.Cache.Remove("Player" + playerID);
                    }
                    Xmoves.Clear();
                    Omoves.Clear();
                    PlayerList.Clear();

                    Clients.All.delNick(playerID);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public bool clearMem()
            {
                try
                {                                      
                    System.Web.HttpContext.Current.Cache.Remove("Omoves");
                    System.Web.HttpContext.Current.Cache.Remove("Xmoves");
                    System.Web.HttpContext.Current.Cache.Remove("MoveType");                  
                    
                    System.Web.HttpContext.Current.Cache.Remove("PlayerID");
                    System.Web.HttpContext.Current.Cache.Remove("Player1");
                    System.Web.HttpContext.Current.Cache.Remove("Player2");                   
                    Xmoves.Clear();
                    Omoves.Clear();
                    PlayerList.Clear();

                    Clients.All.clearCookie();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public bool delAllCookie()
            {
                try
                {
                    if (System.Web.HttpContext.Current.Request.Cookies["TicTacToeNick"] != null)
                    {
                        HttpCookie myCookie = new HttpCookie("TicTacToeNick");
                        myCookie.Expires = DateTime.Now.AddDays(-1d);
                        System.Web.HttpContext.Current.Response.Cookies.Add(myCookie);
                    }
                    Clients.All.clearPage();                    
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }            
            //Disconnected....Olunca
            public override System.Threading.Tasks.Task OnDisconnected()
            {
                return base.OnDisconnected();
            }
        }
        public ActionResult Zen()
        {
            return View();
        }    
    }
}
