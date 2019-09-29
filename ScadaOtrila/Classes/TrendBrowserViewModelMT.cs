using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScadaOtrila.Classes
{
    class TrendBrowserViewModelMT : INotifyPropertyChanged
    {
        #region Props

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private PlotModel plotModel;
        public PlotModel PlotModel { get { return plotModel; } set { plotModel = value; OnPropertyChanged("PlotSalla1"); } }


        #endregion

        private DataOtrilaTableAdapters.TagArchivesTableAdapter trend_logs_ta = new DataOtrilaTableAdapters.TagArchivesTableAdapter();

        private DateTime _from = new DateTime();
        private DateTime _to = new DateTime();
        private string tag1, tag2, tag3;
        private int _updateSall_id = 0;
        public TrendBrowserViewModelMT(int _sallID, DateTime from, DateTime to)
        {
            PlotModel = new PlotModel();
            _updateSall_id = _sallID;
            _from = from;
            _to = to;

            SetupModel();
            LoadData();
        }

        private void SetupModel()
        {
            PlotModel.LegendTitle = "Historia e vlerave";
            PlotModel.LegendOrientation = LegendOrientation.Horizontal;
            PlotModel.LegendPlacement = LegendPlacement.Outside;
            PlotModel.LegendPosition = LegendPosition.TopRight;
            PlotModel.LegendBackground = OxyColor.FromAColor(255, OxyColors.White);
            PlotModel.LegendBorder = OxyColors.Black;
            PlotModel.Background = OxyColors.LightGray;

            var dateAxis = new DateTimeAxis(AxisPosition.Bottom, _from, _to, null, null, DateTimeIntervalType.Auto)
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Position = AxisPosition.Bottom,
                Title = "Date"
            };
            //var dateAxis = new DateTimeAxis(AxisPosition.Bottom, null, null, DateTimeIntervalType.Auto);

            PlotModel.Axes.Add(dateAxis);
            var valueAxis = new LinearAxis(AxisPosition.Left, 0) { MajorGridlineStyle = LineStyle.Dot, MinorGridlineStyle = LineStyle.Dot, Title = "Value", TextColor = OxyColors.Red, MajorGridlineColor = OxyColors.White, MajorGridlineThickness = 1, MinorGridlineColor = OxyColors.White };
            PlotModel.Axes.Add(valueAxis);
        }
        private void LoadData()
        {
            DataOtrila dataOpc = new DataOtrila();

            PlotModel.Series.Clear();
            var lineSerie1 = new OxyPlot.Series.LineSeries
            {
                StrokeThickness = 1,
                CanTrackerInterpolatePoints = false,
                Title = "Temperatura [*C]",
                Smooth = true
            };
            var lineSerie2 = new OxyPlot.Series.LineSeries
            {
                StrokeThickness = 1,
                CanTrackerInterpolatePoints = false,
                Title = "Presioni [Pa]",
                Smooth = true
            };
            var lineSerie3 = new OxyPlot.Series.LineSeries
            {
                StrokeThickness = 1,
                CanTrackerInterpolatePoints = false,
                Title = "Lagështia [%]",
                Smooth = true
            };

            switch (_updateSall_id)
            {
                case 1:
                    {
                        tag1 = "c15842.hall_temp[0]";
                        tag2 = "c15842.hall_pressure[0]";
                        tag3 = "c15842.hall_humid[0]";
                    }
                    break;
                case 2:
                    {
                        tag1 = "c15842.hall_temp[1]";
                        tag2 = "c15842.hall_pressure[1]";
                        tag3 = "c15842.hall_humid[1]";
                    }
                    break;
                case 3:
                    {
                        tag1 = "c15842.hall_temp[2]";
                        tag2 = "c15842.hall_pressure[2]";
                        tag3 = "c15842.hall_humid[2]";
                    }
                    break;

                default:
                    break;
            }

            trend_logs_ta.FillByDateTimeTag(dataOpc.TagArchives, _to, _from, tag1);
            foreach (DataOtrila.TagArchivesRow log in dataOpc.TagArchives.Rows)
            {
                lineSerie1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(log.DateTime), log.Value));
            }
            trend_logs_ta.FillByDateTimeTag(dataOpc.TagArchives, _to, _from, tag2);
            foreach (DataOtrila.TagArchivesRow log in dataOpc.TagArchives.Rows)
            {
                lineSerie2.Points.Add(new DataPoint(DateTimeAxis.ToDouble(log.DateTime), log.Value));
            }
            trend_logs_ta.FillByDateTimeTag(dataOpc.TagArchives, _to, _from, tag3);
            foreach (DataOtrila.TagArchivesRow log in dataOpc.TagArchives.Rows)
            {
                lineSerie3.Points.Add(new DataPoint(DateTimeAxis.ToDouble(log.DateTime), log.Value));
            }

            PlotModel.Series.Add(lineSerie1);
            PlotModel.Series.Add(lineSerie2);
            PlotModel.Series.Add(lineSerie3);
        }
    }
}
