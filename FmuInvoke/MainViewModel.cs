using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using Femyou;
using Femyou.Common;
using FmuInvoke.Models;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FmuInvoke
{
    [POCOViewModel]
    public class MainViewModel
    {
        public MainViewModel()
        {
            ChartModel = new PlotModel();
            Parameters = new ObservableCollection<FmuParameter>();
        }

        public virtual PlotModel ChartModel { get; set; }

        public virtual string FmuPath { get; set; } = string.Empty;

        public virtual ObservableCollection<FmuParameter> Parameters { get; set; }

        public virtual double Progress { get; set; }

        public virtual bool IsRun { get; set; }
        /// <summary>
        /// 仿真时间
        /// </summary>
        public virtual double SimulationTime { get; set; } = 10;

        /// <summary>
        /// 仿真步长
        /// </summary>
        public virtual double SimulationStep { get; set; } = 0.01;

        IModel? model = null;
        /// <summary>
        /// 选择FMU文件
        /// </summary>
        public void SelectFmu()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "FMU文件|*.fmu";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    model = Femyou.Model.Load(openFileDialog.FileName);
                    Parameters.Clear();
                    foreach (var item in model.Variables)
                    {
                        if (item.Value.Causality.Contains("input") || item.Value.Causality.Contains("output")) // AmeSim导出的输入输出参数
                        {
                            FmuParameter parameter = new FmuParameter()
                            {
                                Name = item.Value.Name,
                                Description = item.Value.Description,
                                Causality = item.Value.Causality.Contains("input") ? Causalition.Input : Causalition.Output,
                            };

                            parameter.IsDraw = parameter.Causality == Causalition.Output ? true : false;
                            Parameters.Add(parameter);
                        }
                    }

                    FmuPath = openFileDialog.FileName;
                }
                catch (Exception)
                {
                    MessageBox.Show("FMU读取错误，请确认FMU类型为：Co-Simulation 2.0。");
                }
            }
        }

        int count = 0;
        /// <summary>
        /// 开始运行
        /// </summary>
        public async void Simulation()
        {
            count++;
            IsRun = true;
            Progress = 0;
            await Task.Run(() =>
            {
                try
                {
                    List<IVariable> tempVar = new List<IVariable>();
                    // 先写入参数值
                    using IInstance instance = Tools.CreateInstance(model, $"demo{count}");
                    foreach (var item in Parameters)
                    {
                        var para = model.Variables[item.Name];
                        tempVar.Add(para);
                        try
                        {
                            instance.WriteReal((para, item.ValueSet));
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    Parameters.ForEach(x => x.SimulationData.Clear());

                    instance.StartTime(0.0);
                    double usetime = 0;
                    while (usetime <= SimulationTime)
                    {
                        Progress = usetime / SimulationTime;
                        foreach (var item in tempVar)
                        {
                            var variables = instance.ReadReal(item);
                            var currentVar = Parameters.Where(x => x.Name == item.Name).FirstOrDefault();
                            if (currentVar != null)
                            {
                                currentVar.SimulationData.Add(variables.FirstOrDefault());
                            }
                        }
                        instance.AdvanceTime(SimulationStep);
                        usetime += SimulationStep;
                    }
                    DrawChart();
                }
                catch (Exception)
                {
                    DrawChart();
                    MessageBox.Show("计算时出现异常，提前结束。");
                }

            });

            IsRun = false;
        }

        protected IDispatcherService DispatcherService { get { return this.GetService<IDispatcherService>(); } }
        public async void DrawChart()
        {
            PlotModel tempPlot = new PlotModel();
            tempPlot.IsLegendVisible = true;
            int countdig = CommonTool.GetNumberOfDecimalPlaces(SimulationStep);
            string digitFormat = "0.".PadRight(countdig + 2, '0');

            foreach (var item in Parameters)
            {
                await DispatcherService.BeginInvoke(() =>
                {
                    if (item.IsDraw)
                    {
                        var series1 = new LineSeries
                        {
                            Title = item.Name,
                            MarkerType = MarkerType.None,
                            LegendKey = item.Name,
                            RenderInLegend = true,
                            LineLegendPosition = LineLegendPosition.End,
                            BrokenLineColor = OxyColor.Parse("#FFFFFF"),
                            TrackerFormatString = "{0}\n{1}: {2:" + digitFormat + "}\n{3}: {4:0.00000}"
                        };

                        double x = 0;
                        foreach (var item in item.SimulationData)
                        {
                            series1.Points.Add(new DataPoint(x, item));
                            x += SimulationStep;
                        }
                        tempPlot.Series.Add(series1);
                    }
                });
            }

            ChartModel = tempPlot;
        }


    }
}
