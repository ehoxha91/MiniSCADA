using System;
using System.Collections.Generic;
using OpcLabs.EasyOpc.DataAccess;
using OpcLabs.EasyOpc.DataAccess.Generic;
using OpcLabs.EasyOpc.DataAccess.OperationModel;

namespace OpcOtrilaService
{

    public static class OpcManager
    {
        public static EasyDAClient opcClient;
        public static event EventHandler<OpcTag> OpcManagerTagChanged;
        public static void StartOpcMasteR()
        {
            opcClient = new EasyDAClient();
            opcClient.BrowseBranches("", "CyProOPC.DA2");
            SubscribeTags();
            OpcTag.OpcItemHasChanged += OpcTag_OpcItemHasChanged;
        }

        private static void OpcTag_OpcItemHasChanged(object sender, OpcTag e)
        {
            
            //Everytime a tag changes this event is raised...
            DataOtrilaTableAdapters.TagLiveTableAdapter tagLiveTableAdapter = new DataOtrilaTableAdapters.TagLiveTableAdapter();
            DataOtrila data = new DataOtrila();
            tagLiveTableAdapter.Fill(data.TagLive);

            if (tagLiveTableAdapter.GetTagByName(e.TagName).Count == 0)
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
                int subscriptionid = OpcManager.opcClient.SubscribeItem("", this.OpcServer, this.Tag, this.RefreshRate, ItemChanged );
            }
            catch (Exception)
            {
                //System.Windows.Forms.MessageBox.Show("Failed to subsrcibe!");
            }
        }
        
        private void ItemChanged(object sender,  EasyDAItemChangedEventArgs e)
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
