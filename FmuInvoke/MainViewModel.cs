using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using Femyou;
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

            YFormatter = value => value.ToString("f2");
            Parameters = new ObservableCollection<FmuParameter>();
        }

        public virtual PlotModel ChartModel { get; set; }

        public Func<double, string> YFormatter { get; set; }

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
        public virtual double SimulationStep { get; set; } = 0.1;

        IModel model;
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
                        if (item.Value.Name.Contains("expseu_"))
                        {
                            FmuParameter parameter = new FmuParameter()
                            {
                                Name = item.Value.Name,
                            };
                            Parameters.Add(parameter);
                        }

                    }
                    FmuPath = openFileDialog.FileName;
                }
                catch (Exception)
                {
                    MessageBox.Show("FMU读取错误");
                }
            }
        }

        /// <summary>
        /// 开始运行
        /// </summary>
        public async void Simulation()
        {
            IsRun = true;
            Progress = 0;
            await Task.Run(() =>
            {
                List<IVariable> tempVar = new List<IVariable>();
                // 先写入参数值
                using IInstance instance = Tools.CreateInstance(model, "demo");
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
            });

            IsRun = false;
        }

        protected IDispatcherService DispatcherService { get { return this.GetService<IDispatcherService>(); } }
        public async void DrawChart()
        {
            PlotModel tempPlot = new PlotModel();
            tempPlot.IsLegendVisible = true;
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
                            TrackerFormatString = "{0}\n{1}: {2:0.00}\n{3}: {4:0.0000}"
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
