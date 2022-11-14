using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using System;

namespace Doorways.Entities
{
    public class RecipeLink : LinkedRecipeDetails, INamespacedIDEntity
    {
        public RecipeLink(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log)
        {
        }

        public void CanonicalizeIds(FnCanonicalize fnCanonicalize, string prefix)
        {
            throw new NotImplementedException();
        }
    }
}
