﻿using GaiaCore.Gaia.Tiles;
using GaiaCore.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GaiaCore.Gaia
{
    /// <summary>
    /// 每一局游戏实例化一个Game
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class GaiaGame
    {
        private string m_TailLog=string.Empty;

        public GaiaGame(string[] username)
        {
            GameStatus = new GameStatus();
            GameStatus.PlayerNumber = username.Count();
            FactionList = new List<Faction>();
            FactionNextTurnList = new List<Faction>();
            UserDic = new Dictionary<string, List<Faction>>();
            Username = username;
            foreach(var us in username.Distinct())
            {
                UserDic.Add(us, new List<Faction>());
            }
        }
        public bool ProcessSyntax(string user, string syntax, out string log)
        {
            log = string.Empty;
            if (syntax.StartsWith("#"))
                return false;
            syntax = syntax.ToLower();
            bool ret;
            switch (GameStatus.stage)
            {
                case Stage.RANDOMSETUP:
                    return ProcessSyntaxRandomSetup(syntax, ref log);
                case Stage.FACTIONSELECTION:
                    return ProcessSyntaxFactionSelect(user, syntax, ref log);
                case Stage.INITIALMINES:
                    ret = ProcessSyntaxIntialMines(syntax, ref log);
                    if (ret)
                    {
                        GameStatus.NextPlayerForIntial();
                    }
                    if (ret && FactionList.All(x => x.FinishIntialMines()))
                    {
                        ChangeGameStatus(Stage.SELECTROUNDBOOSTER);
                        GameStatus.SetPlayerIndexLast();
                    }
                    return ret;
                case Stage.SELECTROUNDBOOSTER:
                    ret = ProcessSyntaxRoundBoosterSelect(syntax, ref log);
                    if (ret)
                    {
                        GameStatus.NextPlayerReverse();
                    }
                    ///所有人都选完RBT了
                    if (ret && GameStatus.PlayerIndex + 1 == GameStatus.PlayerNumber)
                    {
                        NewRound();
                    }
                    return ret;
                case Stage.ROUNDSTART:
                    ///吸魔力
                    if (GameSyntax.leechPowerRegex.IsMatch(syntax))
                    {
                        ret = ProcessSyntaxLeechPower(syntax, ref log);
                        return ret;
                    }
                    else
                    {
                        ret = ProcessSyntaxCommand(syntax, ref log);
                        if (ret && GameStatus.IsAllPass())
                        {
                            if (FactionList.All(x => x.LeechPowerQueue.Count == 0))
                            {
                                FactionList = FactionNextTurnList;
                                FactionNextTurnList = new List<Faction>();
                                NewRound();
                            }
                            else
                            {
                                GameStatus.stage = Stage.ROUNDWAITLEECHPOWER;
                            }
                        }
                        else if (ret)
                        {
                            GameStatus.NextPlayer();
                        }

                        return ret;
                    }
                ///只处理吸魔力
                case Stage.ROUNDWAITLEECHPOWER:
                    if (GameSyntax.leechPowerRegex.IsMatch(syntax))
                    {
                        ret = ProcessSyntaxLeechPower(syntax, ref log);
                        if (ret)
                        {
                            FactionList = FactionNextTurnList;
                            FactionNextTurnList = new List<Faction>();
                            NewRound();
                        }
                        else
                        {
                            return ret;
                        }
                    }
                    return false;
                default:
                    return false;
            }
        }

        private void NewRound()
        {
            ChangeGameStatus(Stage.ROUNDINCOME);
            FactionList.ForEach(x => x.CalIncome());
            ChangeGameStatus(Stage.ROUNDSTART);
            GameStatus.NewRoundReset();
        }

        private bool ProcessSyntaxLeechPower(string syntax, ref string log)
        {
            if (!(ValidateSyntaxCommandForLeech(syntax, ref log, out string command, out Faction faction)))
            {
                return false;
            }
            var match = GameSyntax.leechPowerRegex.Match(syntax);
            var isLeech = match.Groups[1].Value.Equals(GameSyntax.leech);
            var power = match.Groups[2].Value.ParseToInt();
            var factionFromStr = match.Groups[3].Value;
            Enum.TryParse(factionFromStr, true, out FactionName factionFrom);
            faction.LeechPower(power, factionFrom, isLeech);
            return true;
        }

        private bool ProcessSyntaxCommand(string syntax, ref string log)
        {
            if (!(ValidateSyntaxCommand(syntax, ref log, out string command, out Faction faction)))
            {
                return false;
            }
            
            var commandList = command.Split('.').Where(x=>!string.IsNullOrEmpty(x));
            //非免费行动只能执行一个
            var NoneFreeActionCount=commandList.Sum(y => GameFreeSyntax.GetRegexList().Exists(x => x.IsMatch(y)) ? 0 : 1);
            if (NoneFreeActionCount != 1)
            {
                log = "能且只能执行一个普通行动";
                return false;
            }
            //faction.Backup();
            if (!ProcessCommandWithBackup(commandList.ToArray(),faction,out log))
            {
                //faction.Rollback();
                return false;
            }
            

            return true;

        }
        
        private bool ProcessCommandWithBackup(string[] commandList,Faction faction,out string log)
        {
            log = string.Empty;
            faction.ResetUnfinishAction();

            foreach (var item in commandList)
            {
                if (GameSyntax.upgradeRegex.IsMatch(item))
                {
                    var match = GameSyntax.upgradeRegex.Match(item);
                    var pos = match.Groups[1].Value;
                    var buildStr = match.Groups[2].Value;
                    ConvertPosToRowCol(pos, out int row, out int col);
                    if (!faction.UpdateBuilding(Map, row, col, buildStr, out log))
                    {
                        return false;
                    }
                }
                else if (GameSyntax.buildRegex.IsMatch(item))
                {
                    var match = GameSyntax.buildRegex.Match(item);
                    var pos = match.Groups[1].Value;
                    ConvertPosToRowCol(pos, out int row, out int col);
                    if (!faction.BuildMine(Map, row, col, out log))
                    {
                        return false;
                    }
                }
                else if (GameFreeSyntax.QICShip.IsMatch(item))
                {
                    var match = GameFreeSyntax.QICShip.Match(item);
                    var num = match.Groups[1].Value.ParseToInt(0);
                    if (!faction.SetQICShip(num, out log))
                    {
                        return false;
                    }
                }
                else if (GameFreeSyntax.transformRegex.IsMatch(item))
                {
                    var match = GameFreeSyntax.transformRegex.Match(item);
                    var num = match.Groups[1].Value.ParseToInt(0);
                    if (!faction.SetTransformNumber(num, out log))
                    {
                        return false;
                    }
                }
                else if (GameFreeSyntax.getTechTilesRegex.IsMatch(item))
                {
                    var techTileStr = item.Substring(1);
                    GameTiles tile;
                    if (ATTList.Exists(x => string.Compare(x.GetType().Name, techTileStr, true) == 0))
                    {
                        tile = ATTList.Find(x => string.Compare(x.GetType().Name, techTileStr, true) == 0);
                    }
                    else if (STT3List.Exists(x => string.Compare(x.GetType().Name, techTileStr, true) == 0))
                    {
                        faction.TechTrachAdv++;
                        tile = STT3List.Find(x => string.Compare(x.GetType().Name, techTileStr, true) == 0);
                    }
                    else if (STT6List.Exists(x => string.Compare(x.GetType().Name, techTileStr, true) == 0))
                    {
                        tile = STT6List.Find(x => string.Compare(x.GetType().Name, techTileStr, true) == 0);
                    }
                    else
                    {
                        log = string.Format("{0}这块板子不存在", techTileStr);
                        return false;
                    }
                    Action queue = () =>
                    {
                        faction.GameTileList.Add(tile);
                        if (ATTList.Exists(x => string.Compare(x.GetType().Name, techTileStr, true) == 0))
                        {
                            ATTList.Remove(ATTList.Find(x => string.Compare(x.GetType().Name, techTileStr, true) == 0));
                        }
                    };

                    if (STT6List.Exists(x => string.Compare(x.GetType().Name, techTileStr, true) == 0))
                    {
                        var index = STT6List.FindIndex(x => string.Compare(x.GetType().Name, techTileStr, true) == 0);
                        faction.LimitTechAdvance = Faction.ConvertTechIndexToStr(index);
                    }
                    faction.ActionQueue.Enqueue(queue);
                    faction.TechTilesGet--;
                }
                else if (GameFreeSyntax.advTechRegex.IsMatch(item))
                {

                    string tech;
                    if (faction.TechTrachAdv == 0)
                    {
                        log = "不能推进科技条";
                        return false;
                    }
                    if (string.IsNullOrEmpty(faction.LimitTechAdvance)&& GameFreeSyntax.advTechRegex2.IsMatch(item))
                    {
                        var match = GameFreeSyntax.advTechRegex2.Match(item);
                        tech = match.Groups[1].Value;
                    }else if (!string.IsNullOrEmpty(faction.LimitTechAdvance))
                    {
                        tech = faction.LimitTechAdvance;
                    }
                    else
                    {
                        log = "请检查语句语法格式";
                        return false;
                    }

                    if (faction.IsIncreateTechValide(tech))
                    {
                        Action queue = () =>
                        {
                            faction.IncreaseTech(tech);
                        };
                        faction.ActionQueue.Enqueue(queue);
                        faction.TechTrachAdv--;
                    }
                    else
                    {
                        log = "此科技条不能继续上升";
                        return false;
                    }
                }
                else if (GameSyntax.passRegex.IsMatch(item)){
                    var match = GameSyntax.passRegex.Match(item);
                    var rbtStr=match.Groups[1].Value;
                    FactionNextTurnList.Add(faction);
                    ProcessGetRoundBooster(rbtStr, faction, out log);
                    GameStatus.SetPassPlayerIndex(FactionList.IndexOf(faction));
                }
                else
                {
                    log = "语句还不支持";
                    return false;
                }
            }
            if (faction.IsExitUnfinishFreeAction(out log))
            {
                return false;
            }

            foreach (var item in faction.ActionQueue)
            {
                item.Invoke();
            }

            return true;
        }


        private bool ProcessSyntaxRoundBoosterSelect(string syntax, ref string log)
        {
            if (!(ValidateSyntaxCommand(syntax, ref log, out string command, out Faction faction)))
            {
                return false;
            }
            ///处理SelectRBT命令
            if (!GameSyntax.RBTRegex.IsMatch(command))
            {
                log = "命令错误";
                return false;
            }
            var rbtStr = command.Substring(1);

            ProcessGetRoundBooster(rbtStr, faction, out log);
            return true;
        }

        private bool ProcessGetRoundBooster(string rbtStr,Faction faction,out string log)
        {
            log = string.Empty;
            var rbt = RBTList.Find(x => x.GetType().Name.Equals(rbtStr, StringComparison.OrdinalIgnoreCase));
            if (rbt == null)
            {
                log = string.Format("{0}板子不存在", rbtStr);
                return false;
            }
            if(faction.GameTileList.Exists(x=>x is RoundBooster))
            {
                RBTList.Add(faction.GameTileList.Find(x => x is RoundBooster) as RoundBooster);
            }
            faction.GameTileList.Add(rbt);
            RBTList.Remove(rbt);
            return true;
        }

        private bool ProcessSyntaxIntialMines(string syntax, ref string log)
        {
            if (!(ValidateSyntaxCommand(syntax, ref log, out string command, out Faction faction)))
            {
                return false;
            }
            ///处理Build命令
            if (!GameSyntax.buildRegex.IsMatch(command))
            {
                log = "命令错误";
                return false;
            }
            ///Build A2
            var position = command.Substring(GameSyntax.factionSelection.Length + 1);

            ConvertPosToRowCol(position, out int row, out int col);

            if (faction.BuildIntialMine(Map, row, col, out log))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private static void ConvertPosToRowCol(string position, out int row, out int col)
        {
            row = position.Substring(0, 1).ToCharArray().First() - 'a';
            col = position.Substring(1).ParseToInt(0);
        }
        public bool ValidateSyntaxCommand(string syntax, ref string log, out string command, out Faction faction)
        {
            if (!ValidateSyntaxCommandForLeech(syntax, ref log, out command, out faction))
            {
                return false;
            }

            if (!(FactionList[GameStatus.PlayerIndex] == faction))
            {
                log = string.Format("不是种族{0}行动轮,是{1}行动轮", faction.FactionName, FactionList[GameStatus.PlayerIndex].FactionName.ToString());
                return false;
            }

            if (faction.LeechPowerQueue.Count != 0)
            {
                log = "必须先执行吸取魔力行动";
                return false;
            }
            return true;
        }
        public bool ValidateSyntaxCommandForLeech(string syntax, ref string log, out string command, out Faction faction)
        {
            command = string.Empty;
            faction = null;
            if (!GameSyntax.commandRegex.IsMatch(syntax))
            {
                log = "格式为 种族:命令";
                return false;
            }

            var factionName = syntax.Split(':').First();
            if (!Enum.TryParse(factionName, true, out FactionName result))
            {
                log = "FactionName is wrong";
                return false;
            }

            faction = FactionList.Find(x => x.FactionName == result);
            if (faction == null)
            {
                log = "FactionName doesn't exit";
                return false;
            }

            command = syntax.Split(':').Last();
            return true;
        }

        private bool ProcessSyntaxFactionSelect(string user,string syntax, ref string log)
        {
            log = string.Empty;
            if (GameSyntax.factionSelectionRegex.IsMatch(syntax))
            {
                var faction = syntax.Substring(GameSyntax.factionSelection.Length + 1);
                if (Enum.TryParse(faction, true, out FactionName result))
                {
                    if (!FactionList.Exists(x => x.FactionName == result))
                    {

                        SetupFaction(user,result);
                    }
                    else
                    {
                        log = "FactionName has been choosen!";
                        return false;
                    }
                    if (FactionList.Count == 4)
                    {
                        ChangeGameStatus(Stage.INITIALMINES);
                    }
                    return true;
                }
                else
                {
                    log = "FactionName is wrong";
                }
            }
            return false;
        }

        private bool ProcessSyntaxRandomSetup(string syntax, ref string log)
        {
            log = string.Empty;
            if (GameSyntax.setupGameRegex.IsMatch(syntax))
            {
                var seed = syntax.Substring(GameSyntax.setupGame.Length).ParseToInt(0);
                GameStart(syntax, seed);
                ChangeGameStatus(Stage.FACTIONSELECTION);
                return true;
            }
            return false;
        }

        public void Syntax(string syntax, out string log, string user = "")
        {
            if (ProcessSyntax(user,syntax, out log))
            {
                UserActionLog += syntax.AddEnter();
                UserActionLog += m_TailLog;
                m_TailLog = string.Empty;
            }
        }

        private void SetupFaction(string user,FactionName faction)
        {
            switch (faction)
            {
                case FactionName.Terraner:
                    FactionList.Add(new Terraner(this));
                    break;
                case FactionName.Taklons:
                    FactionList.Add(new Taklons(this));
                    break;
                case FactionName.MadAndroid:
                    FactionList.Add(new MadAndroid(this));
                    break;
                case FactionName.Geoden:
                    FactionList.Add(new Geoden(this));
                    break;
                default:
                    break;
            };
            if (string.IsNullOrEmpty(user))
            {
                user = Username[FactionList.Count - 1];
            }
            UserDic[user].Add(FactionList.Last());
        }

        private void GameStart(string syntax, int i = 0)
        {
            Seed = i == 0 ? RandomInstance.Next(int.MaxValue) : i;
            var random = new Random(Seed);
            Map = new MapMgr().GetRandomMap(random);
            ATTList = (from items in ATTMgr.GetRandomList(6, random) orderby items.GetType().Name.Remove(0, 3).ParseToInt(-1) select items).ToList();
            STT6List = STTMgr.GetRandomList(6, random);
            STT3List = (from items in STTMgr.GetOtherList(STT6List) orderby items.GetType().Name.Remove(0, 3).ParseToInt(-1) select items).ToList();
            RSTList = RSTMgr.GetRandomList(6, random);
            FSTList = new List<FinalScoring>();
            FSTList.Add(new FST1());
            RBTList = (from items in RBTMgr.GetRandomList(4 + 3, random) orderby items.GetType().Name.Remove(0, 3).ParseToInt(-1) select items).ToList();
            ALTList = ALTMgr.GetList();
            AllianceTileForKnowledge = ALTList.RandomRemove(random);
        }
        private void ChangeGameStatus(Stage stage)
        {
            m_TailLog += "#" + stage.ToString().AddEnter();
            GameStatus.stage = stage;
        }

        public void SetLeechPowerQueue(FactionName factionName,int row,int col)
        {
            foreach(var item in FactionList.Where(x => !x.FactionName.Equals(factionName)))
            {
                var power=Map.CalHighestPowerBuilding(row,col,item.FactionName);
                if (power != 0)
                {
                    item.LeechPowerQueue.Add(new Tuple<int, FactionName>(power, factionName));
                }
            }
        }

        /// <summary>
        /// 实例化四个玩家
        /// </summary>
        public List<Faction> FactionList { set; get; }
        public List<Faction> FactionNextTurnList { set; get; }
        #region 存档需要save的内容
        [JsonProperty]
        /// <summary>
        /// 在游戏开始的时候实例化一个Map
        /// </summary>
        public Map Map { set; get; }
        /// <summary>
        /// 游戏的一些公共状态包括现在第几轮轮到哪个玩家行动等等
        /// </summary>
        public GameStatus GameStatus { set; get; }
        /// <summary>
        /// StandardTechList
        /// </summary>
        public List<StandardTechnology> STT6List { set; get; }
        /// <summary>
        /// StandardTechListWithNoKnowledge
        /// </summary>
        public List<StandardTechnology> STT3List { set; get; }
        /// <summary>
        /// AdvanceTechList
        /// </summary>
        public List<AdavanceTechnology> ATTList { set; get; }
        /// <summary>
        /// RoundScoringList
        /// </summary>
        public List<RoundScoring> RSTList { set; get; }
        /// <summary>
        /// FinalScoringList
        /// </summary>
        public List<FinalScoring> FSTList { set; get; }
        /// <summary>
        /// RoundBoostList
        /// </summary>
        public List<RoundBooster> RBTList { set; get; }
        /// <summary>
        /// AlianceList
        /// </summary>
        public List<AllianceTile> ALTList { set; get; }
        public AllianceTile AllianceTileForKnowledge { set; get; }
        [JsonProperty]
        public string UserActionLog { set; get; }
        [JsonProperty]
        public int Seed { set; get; }
        [JsonProperty]
        public string[] Username { set; get; }
        public Dictionary<string,List<Faction>> UserDic { set; get; }
        #endregion
    }
}
