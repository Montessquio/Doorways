using SecretHistories.Entities;
using SecretHistories.Fucine.DataImport;
using SecretHistories.Fucine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Entities
{
    public class Aspect : Card, INamespacedIDEntity
    {
        public new bool IsAspect
        {
            get
            {
                return true;
            }
            set { }
        }

        public override string Label { get; set; }
        public override string Description { get; set; }

        public Aspect(string baseid, string title, string description) : base(new EntityData(), new ContentImportLog())
        {
            this.Id = baseid;
            this.Label = title;
            this.Description = description;
        }
    }
}
