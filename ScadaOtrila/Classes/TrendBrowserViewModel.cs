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
      class TrendBrowserViewModel : INotifyPropertyChanged
       {
           #region Props

           public event PropertyChangedEventHandler PropertyChanged;

           protected virtual void OnPropertyChanged(string propertyName)
           {
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
           }

           private PlotModel plotModel;
           public PlotModel PlotModel { get { return plotModel; } set { plotModel = value; OnPropertyChanged("plot1"); } }

        #endregion

           private DataOtrilaTableAdapters.TagArchivesTableAdapter trend_logs_ta = new DataOtrilaTableAdapters.TagArchivesTableAdapter();

           private DateTime _from = new DateTime();
           private DateTime _to = new DateTime();
           private string Tags;
           public TrendBrowserViewModel(string tags, DateTime from, DateTime to)
           {
               PlotModel = new OxyPlot.PlotModel();
               Tags = tags;
               _from = from;
               _to = to;

               SetupModel();
               LoadData();
           }

           private void SetupModel()
           {
               PlotModel.LegendTitle = "Trend Data";
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
               PlotModel.Series.Clear();
                DataOtrila dataOpc = new DataOtrila();

               var lineSerie = new OxyPlot.Series.LineSeries
               {
                   StrokeThickness = 1,
                   CanTrackerInterpolatePoints = false,
                   Title = Tags,
                   Smooth = false
               };

               trend_logs_ta.FillByDateTimeTag(dataOpc.TagArchives, _to, _from, Tags);
               foreach (DataOtrila.TagArchivesRow log in dataOpc.TagArchives.Rows)
               {
                   lineSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(log.DateTime), log.Value));
               }

               PlotModel.Series.Add(lineSerie);
           }
       } 
}
