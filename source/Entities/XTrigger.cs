﻿using SecretHistories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Entities
{
    public class XTrigger : MorphDetails, INamespacedIDEntity
    {


        public XTrigger() { }
        public XTrigger(MorphDetails original) { }

        public void CanonicalizeIds(FnCanonicalize fnCanonicalize, string prefix)
        {
            throw new NotImplementedException();
        }
    }
}
