﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace GaiaCore.Gaia
{
    public class Taklons : Faction
    {
        public Taklons(GaiaGame gg) : base(FactionName.Taklons, gg)
        {
            this.ChineseName = "利爪族";
            this.ColorCode = colorList[4];
            this.ColorMap = colorMapList[4];
            PowerToken1 += 1;
            BigStone = 1;
        }
        /// <summary>
        /// 所谓的智慧之石 1表示在1区 2表示在2区 3表示3区
        /// </summary>
        public int BigStone { set; get; }
        public override Terrain OGTerrain { get => Terrain.Brown; }
        public int BigStoneBackup { get; private set; }

        public override int PowerIncrease(int i)
        {
            if (m_powerToken1 > i)
            {
                BigStone = 2;
            }
            else if (m_powerToken1 * 2 + m_powerToken2 > i)
            {
                BigStone = 3;
            }
            else
            {
                BigStone = 3;
            }
            return base.PowerIncrease(i);
        }

        public override int PowerTokenRepresent
        {
            get
            {
                if (BigStone == 3)
                {
                    return base.PowerTokenRepresent + 2;
                }
                else
                {
                    return base.PowerTokenRepresent;
                }
            }
        }

        public override void PowerUse(int v)
        {
            if (BigStone == 3)
            {
                PowerToken3 -= v - 2;
                PowerToken1 += v - 2;
                BigStone = 1;
            }
            else
            {
                base.PowerUse(v);
            }
        }

        public override void PowerBurnSpecialPreview(int v)
        {
            if (BigStone == 2)
            {
                BigStone = 3;
                BigStoneBackup = 2;
            }
            base.PowerBurnSpecialPreview(v);
        }

        public override void PowerBurnSpecialActual(int v)
        {
            BigStoneBackup = 0;
            base.PowerBurnSpecialActual(v);
        }

        public override void ResetUnfinishAction()
        {
            if (BigStoneBackup != 0)
            {
                BigStone = BigStoneBackup;
                BigStoneBackup = 0;
            }
            base.ResetUnfinishAction();
        }

        public override bool ConvertOneResourceToAnother(int rFNum, string rFKind, int rTNum, string rTKind, out string log, int? rTNum2 = default(int?), string rTKind2 = null, int? rFNum2 = null, string rFKind2 = null)
        {
            log = string.Empty;
            var str = rFKind + rFKind2 + rTKind;
            switch (str)
            {
                case "bso":
                    if (rFNum != rTNum)
                    {
                        log = "兑换比例为1：1";
                        return false;
                    }
                    if (rFNum != 1)
                    {
                        log = "只有一个智慧石";
                        return false;
                    }
                    if (BigStone != 3)
                    {
                        log = "智慧石不在3区";
                        return false;
                    }
                    if (PowerToken3 < 1)
                    {
                        log = "魔力值不够";
                        return false;
                    }
                    TempPowerToken3 -= 1;
                    TempPowerToken1 += 1;
                    TempOre += 1;
                    BigStone = 1;
                    BigStoneBackup = 3;
                    Action action = () =>
                    {
                        PowerToken3 = PowerToken3;
                        PowerToken1 = PowerToken1;
                        Ore = Ore;
                        BigStoneBackup = 0;
                    };
                    ActionQueue.Enqueue(action);
                    return true;
                case "bsc":
                    if (rFNum*3 != rTNum)
                    {
                        log = "兑换比例为1：3";
                        return false;
                    }
                    if (rFNum != 1)
                    {
                        log = "只有一个智慧石";
                        return false;
                    }
                    if (BigStone != 3)
                    {
                        log = "智慧石不在3区";
                        return false;
                    }
                    if (PowerToken3 < 1)
                    {
                        log = "魔力值不够";
                        return false;
                    }
                    TempPowerToken3 -= 1;
                    TempPowerToken1 += 1;
                    TempCredit += 3;
                    BigStone = 1;
                    BigStoneBackup = 3;
                    action = () =>
                    {
                        PowerToken3 = PowerToken3;
                        PowerToken1 = PowerToken1;
                        Credit = Credit;
                        BigStoneBackup = 0;
                    };
                    ActionQueue.Enqueue(action);
                    return true;
                case "bspwq":
                    if (rFNum * 3 + rFNum2 != rTNum * 4)
                    {
                        log = "兑换比例为1:1:1";
                        return false;
                    }
                    if (rFNum != 1)
                    {
                        log = "只有一个智慧石";
                        return false;
                    }
                    if (BigStone != 3)
                    {
                        log = "智慧石不在3区";
                        return false;
                    }
                    if (PowerToken3 < rFNum2+1)
                    {
                        log = "魔力值不够";
                        return false;
                    }
                    TempPowerToken3 -= rFNum2.GetValueOrDefault()+1;
                    TempPowerToken1 += rFNum2.GetValueOrDefault() + 1;
                    TempQICs += 1;
                    BigStone = 1;
                    BigStoneBackup = 3;
                    action = () =>
                    {
                        PowerToken3 = PowerToken3;
                        PowerToken1 = PowerToken1;
                        QICs = QICs;
                        BigStoneBackup = 0;
                    };
                    ActionQueue.Enqueue(action);
                    return true;
                case "bspwk":
                    if (rFNum * 3 + rFNum2 != rTNum * 4)
                    {
                        log = "兑换比例为1:1:1";
                        return false;
                    }
                    if (rFNum != 1)
                    {
                        log = "只有一个智慧石";
                        return false;
                    }
                    if (BigStone != 3)
                    {
                        log = "智慧石不在3区";
                        return false;
                    }
                    if (PowerToken3 < rFNum2 + 1)
                    {
                        log = "魔力值不够";
                        return false;
                    }
                    TempPowerToken3 -= rFNum2.GetValueOrDefault() + 1;
                    TempPowerToken1 += rFNum2.GetValueOrDefault() + 1;
                    TempKnowledge += 1;
                    BigStone = 1;
                    BigStoneBackup = 3;
                    action = () =>
                    {
                        PowerToken3 = PowerToken3;
                        PowerToken1 = PowerToken1;
                        Knowledge = Knowledge;
                        BigStoneBackup = 0;
                    };
                    ActionQueue.Enqueue(action);
                    return true;
                case "bspwt":
                    if (rFNum  != rTNum )
                    {
                        log = "兑换比例为1:1";
                        return false;
                    }
                    if (rFNum != 1)
                    {
                        log = "只有一个智慧石";
                        return false;
                    }
                    if (BigStone != 3)
                    {
                        log = "智慧石不在3区";
                        return false;
                    }
                    if (PowerToken3 < rFNum2 + 1)
                    {
                        log = "魔力值不够";
                        return false;
                    }
                    TempPowerToken3 -=  1;
                    TempPowerToken1 +=  2;
                    BigStone = 1;
                    BigStoneBackup = 3;
                    action = () =>
                    {
                        PowerToken3 = PowerToken3;
                        PowerToken1 = PowerToken1;
                        BigStoneBackup = 0;
                    };
                    ActionQueue.Enqueue(action);
                    return true;
            }
            return base.ConvertOneResourceToAnother(rFNum, rFKind, rTNum, rTKind, out log, rTNum2, rTKind2);
        }
    }
}
