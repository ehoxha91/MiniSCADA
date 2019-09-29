using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpcLabs.EasyOpc.DataAccess.OperationModel;
using OpcLabs.EasyOpc.DataAccess;
using System.Windows;
using System.Threading;

namespace ScadaOtrila.Classes
{

    public static class OpcManager
    {
        public static EasyDAClient opcClient;
        public static EasyDAClient writterClient;
        public static event EventHandler<OpcTag> OpcManagerTagChanged;
        private static System.Timers.Timer opcStackTimer;
        public static void StartOpcMasteR()
        {
            opcClient = new EasyDAClient();
            opcClient.BrowseBranches("", "CyProOPC.DA2");
            SubscribeTags();

            //Remove all previous commands
            (new DataOtrilaTableAdapters.OpcStackOtrilaTableAdapter()).ClearStack();

            OpcTag.OpcItemHasChanged += OpcTag_OpcItemHasChanged;
            opcStackTimer = new System.Timers.Timer(1000);
            opcStackTimer.Elapsed += OpcStackTimer_Elapsed;
            opcStackTimer.Start();
        }

        public static void StopThread()
        {
            try
            {
                opcStackTimer.Stop();
                opcStackTimer.Dispose();
            }
            catch { }
        }

        private static void OpcStackTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                opcStackTimer.Stop();

                DataOtrila data = new DataOtrila();
                DataOtrilaTableAdapters.OpcStackOtrilaTableAdapter stack_ta = new DataOtrilaTableAdapters.OpcStackOtrilaTableAdapter();
                stack_ta.Fill(data.OpcStackOtrila);

                foreach (DataOtrila.OpcStackOtrilaRow row in data.OpcStackOtrila.Rows)
                {
                    if(DateTime.Now.Minute == row.Time.Minute && DateTime.Now.Hour == row.Time.Hour)
                    {
                        if(WriteTag(row.OpcServer, row.Tag, row.NewValue))
                        {
                            stack_ta.DeleteFromStack(row.ID);
                        }
                    }
                    else //old un-successful command, delete it.
                    {
                        stack_ta.DeleteFromStack(row.ID);
                    }
                }

                opcStackTimer.Start();
            }
            catch { }
        }

        private static void OpcTag_OpcItemHasChanged(object sender, OpcTag e)
        {
            //Everytime a tag changes this event is raised...
            DataOtrilaTableAdapters.TagLiveTableAdapter tagLiveTableAdapter = new DataOtrilaTableAdapters.TagLiveTableAdapter();
            DataOtrila data = new DataOtrila();
            tagLiveTableAdapter.Fill(data.TagLive);
            

            if (tagLiveTableAdapter.GetByTagName(e.TagName).Count == 0)
            {
                bool typ = false;
                if (e.TagType == TagTypeE.Analog)
                    typ = true;
                tagLiveTableAdapter.Insert(e.TagID, e.TagName, e.NewValue, typ, false, DateTime.Now);
                tagList.Add(e);
            }
            else
            {
                tagLiveTableAdapter.UpdateValue(e.NewValue, DateTime.Now.ToString(), e.TagID);  //Update table
                //Update the list also.
                for (int i = 0; i < tagList.Count; i++)
                {
                    if (e.TagID == tagList[i].TagID)
                        tagList[i].NewValue = e.NewValue;
                }
            }
            //Archive here
            if (e.Archiving == true)
            {
                (new DataOtrilaTableAdapters.TagArchivesTableAdapter()).Insert(e.TagID, e.TagName, e.Tag, e.OpcServer, DateTime.Now, e.NewValue);
            }

        }

        private static List<OpcTag> tagList;        //List where we keep our tags for manipulation.
        private static void SubscribeTags()
        {
            DataOtrilaTableAdapters.TagsTableAdapter tagsTableAdapter = new DataOtrilaTableAdapters.TagsTableAdapter();
            tagList = new List<OpcTag>();
            DataOtrila data = new DataOtrila();
            tagsTableAdapter.Fill(data.Tags);
            foreach (DataOtrila.TagsRow row in data.Tags.Rows)
            {
                TagTypeE tagTypeE = TagTypeE.Digital;
                if (row.Type == 2)
                {
                    tagTypeE = TagTypeE.Analog;
                }

                OpcTag _tag = new OpcTag() {
                    TagID = row.ID,
                    Tag = row.Tag,
                    TagName = row.Name,
                    OpcServer = row.OpcServer,
                    RefreshRate = row.RefreshTime,
                    Scaling = row.Scale,
                    Description = row.Description,
                    Unit = row.Unit,
                    TagType =tagTypeE,
                    Archiving =  row.Archive
                };
                _tag.StartSubscription();
                tagList.Add(_tag);
            }
        }

        public static bool WriteTag(string _opcserver, string _tag, object _valuetowrite)
        {
            try
            {

                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        opcClient.WriteItemValue("CyProOPC.DA2", _tag, _valuetowrite);
                        string _event = "Komanda u ekzekutua me sukses! Tag: " +_tag + "; Vlera: " +Convert.ToString(_valuetowrite)+".";
                        (new DataOtrilaTableAdapters.EventLogsTableAdapter()).Insert(DateTime.Now, "System", _event);
                        return true;
                    }

                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                    }
                }

            }
            catch { }
            
            return false;
        }
    }

    public class OpcTag
    {   
        //Whoever want's to listen to changes should subscribe here
        public static event EventHandler<OpcTag> OpcItemHasChanged;     

        #region Tag Properties
        public int TagID { get; set; }
        public string TagName { get; set; } = "";
        public string Tag { get; set; }
        public string OpcServer { get; set; }
        public string Unit { get; set; }
        public float Scaling { get; set; }
        public TagTypeE TagType { get; set; } = TagTypeE.Digital;
        public int RefreshRate { get; set; }
        public int NewValue { get; set; }
        public float LastValue { get; set; }
        public int SubscriptionId { get; set; } = -1;
        public string Description { get; set; }

        public bool Archiving { get; set; } = true;
        #endregion

        public void StartSubscription()
        {
            try
            {
                int subscriptionid = OpcManager.opcClient.SubscribeItem("", this.OpcServer, this.Tag, 500, ItemChanged );
            }
            catch (Exception)
            {
                //System.Windows.Forms.MessageBox.Show("Failed to subsrcibe!");
            }
        }
        
        private void ItemChanged(object sender, EasyDAItemChangedEventArgs e)
        {
            try
            {
                if(e.Exception == null)
                {
                    if(e.Vtq.Quality.IsGood)
                    {
                        this.NewValue = Convert.ToInt32(e.Vtq.Value);

                        //Rise the event, as argument send the tag object.
                        EventHandler<OpcTag> h = OpcItemHasChanged; 
                        if (h != null)
                            h.Invoke(null, this);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void Unsubscribe()
        {
            OpcManager.opcClient.UnsubscribeItem(SubscriptionId);
        }
    }
    public enum TagTypeE
    {
        Digital = 1,
        Analog = 2
    }
}
