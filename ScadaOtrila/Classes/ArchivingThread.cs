using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScadaOtrila.Classes
{
    public class ArchivingThread
    {
        public static bool keepArchiving = true;
        public static DataOtrila dataOtrila = new DataOtrila();
        public static DataOtrilaTableAdapters.TagsTableAdapter tags_ta = new DataOtrilaTableAdapters.TagsTableAdapter();
        public static void StartArchiving()
        {
            while(keepArchiving)
            {
                try
                {
                    tags_ta.Fill(dataOtrila.Tags); //Fill the table adapter

                    foreach (DataOtrila.TagsRow tag in dataOtrila.Tags.Rows)  //Check all tags, archive the ones we want to...
                    {
                        if (tag.Archive)
                        {
                            object _tagLive = ((new DataOtrilaTableAdapters.TagLiveTableAdapter()).GetByTagName(tag.Name));
                        }
                    }

                    Thread.Sleep(2000);
                }
                catch { }
            }
        }
    }
}
