using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BalanceUI.Service
{
    public class FormulaParser
    {
        /// <summary>
        /// 公式
        /// </summary>
        private string _formula;
        /// <summary>
        /// S变量列表
        /// </summary>
        private IList<string> _sList = new List<string>();
        /// <summary>
        /// M变量列表
        /// </summary>
        private IList<string> _mList = new List<string>();
        /// <summary>
        /// 所有的公式因子
        /// </summary>
        //private IList<string> _allFactorList = new List<string>();


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="myFormula">公式</param>
        public FormulaParser(string myFormula)
        {
            for (int i = myFormula.Count() - 1; i >= 0; i--)
            {
                if (myFormula.ElementAt(i) == ' ')
                {
                    myFormula = myFormula.Remove(i, 1);
                }
            }

            _formula = myFormula;
        }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public FormulaParser() { }
        /// <summary>
        /// 获取S变量
        /// </summary>
        /// <returns></returns>
        public IList<string> GetSList()
        {
            IList<string> allFactorList = GetAllFactorList();
            foreach (string item in allFactorList)
            {
                if (item.Contains('S') && !_sList.Contains(item.Trim()))
                {
                    _sList.Add(item.Trim());
                }
            }
            return _sList;
        }
        /// <summary>
        /// 获取M变量
        /// </summary>
        /// <returns></returns>
        public IList<string> GetMList()
        {
            IList<string> allFactorList = GetAllFactorList();
            foreach (string item in allFactorList)
            {
                if (item.Contains('M') && !_mList.Contains(item.Trim()))
                {
                    _mList.Add(item.Trim());
                }
            }
            return _mList;
        }
        /// <summary>
        /// 获取所有的公式因子
        /// </summary>
        /// <returns></returns>
        public IList<string> GetAllFactorList()
        {
            IList<string> allFactorList = new List<string>();
            IEnumerable<string> factorList = Regex.Split(_formula, @"[+\-*/()]+")
                                                .Except((IEnumerable<string>)new string[] { "" })
                                                .Select(p => p = Regex.Replace(p, @"^([0-9]+)([\.]([0-9]+))?$", ""))
                                                .Except((IEnumerable<string>)new string[] { "" });
            foreach (string item in factorList)
            {
                allFactorList.Add(item.Trim());
            }
            //_allFactorList = (IList<string>)factorList;
            return allFactorList;
        }
        public IList<string> GetAllFactorList(string myFormula)
        {
            IList<string> allFactorList = new List<string>();
            IEnumerable<string> factorList = Regex.Split(myFormula, @"[+\-*/()]+")
                                                .Except((IEnumerable<string>)new string[] { "" })
                                                .Select(p => p = Regex.Replace(p, @"^([0-9]+)([\.]([0-9]+))?$", ""))
                                                .Except((IEnumerable<string>)new string[] { "" });
            foreach (string item in factorList)
            {
                allFactorList.Add(item.Trim());
            }
            //_allFactorList = (IList<string>)factorList;
            return allFactorList;
        }
        /// <summary>
        /// 获取输入量
        /// </summary>
        /// <returns></returns>
        public IList<string> GetInputFactorList()
        {
            IList<string> inputFactorList = new List<string>();
            string[] regexStrArray = { @"^{0}", @"\+{0}" };
            IList<string> allFactorList = GetAllFactorList();
            foreach (string factory in allFactorList)
            {
                foreach (string item in regexStrArray)
                {
                    if (Regex.IsMatch(_formula, string.Format(item, factory)))
                    {
                        inputFactorList.Add(factory);
                    }
                }
            }
            return inputFactorList;
        }

        /// <summary>
        /// 获取消耗量
        /// </summary>
        /// <returns></returns>
        public IList<string> GetOutputFactorList()
        {
            IList<string> outputFactorList = new List<string>();
            string regexStr = @"\-{0}";
            IList<string> allFactorList = GetAllFactorList();
            foreach (string factory in allFactorList)
            {
                if (Regex.IsMatch(_formula, string.Format(regexStr, factory)))
                {
                    outputFactorList.Add(factory);
                }
            }
            return outputFactorList;
        }


        /// <summary>
        /// 获取输入量
        /// </summary>
        /// <param name="materialType">类型:S,M</param>
        /// <returns></returns>
        public IList<string> GetInputFactorList(string materialType)
        {
            IList<string> inputFactorList = new List<string>();
            string[] regexStrArray = { @"^{0}", @"\+{0}" };
            IList<string> allFactorList = GetAllFactorList();
            foreach (string factory in allFactorList)
            {
                foreach (string item in regexStrArray)
                {
                    if (Regex.IsMatch(_formula, string.Format(item, factory)))
                    {
                        if (factory.Contains(materialType))
                        {
                            inputFactorList.Add(factory);
                        }
                    }
                }
            }
            return inputFactorList;
        }

        /// <summary>
        /// 获取消耗量
        /// </summary>
        /// <param name="materialType">类型:S,M</param>
        /// <returns></returns>
        public IList<string> GetOutputFactorList(string materialType)
        {
            IList<string> outputFactorList = new List<string>();
            string regexStr = @"\-{0}";
            IList<string> allFactorList = GetAllFactorList();
            foreach (string factory in allFactorList)
            {
                if (Regex.IsMatch(_formula, string.Format(regexStr, factory)))
                {
                    if (factory.Contains(materialType))
                    {
                        outputFactorList.Add(factory);
                    }
                }
            }
            return outputFactorList;
        }
    }
}
