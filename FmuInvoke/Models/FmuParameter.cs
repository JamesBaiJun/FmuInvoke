using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FmuInvoke.Models
{
    public class FmuParameter : BindableBase
    {
        public FmuParameter()
        {
            SimulationData = new List<double>();
        }
        /// <summary>
        /// 变量名
        /// </summary>
        public string Name
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        /// <summary>
        /// 设置值
        /// </summary>
        public double ValueSet
        {
            get { return GetValue<double>(); }
            set { SetValue(value); }
        }


        /// <summary>
        /// 是否加入绘图
        /// </summary>
        public bool IsDraw
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        /// <summary>
        /// 仿真数据
        /// </summary>
        public List<double> SimulationData
        {
            get { return GetValue<List<double>>(); }
            set { SetValue(value); }
        }
    }
}
