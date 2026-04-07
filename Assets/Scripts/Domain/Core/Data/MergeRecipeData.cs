using System.Collections.Generic;

namespace Domain.Core.Data
{
    [System.Serializable]
    public class MergeRecipeData
    {
        public string RecipeId;
        public string ComponentA_Id;
        public string ComponentB_Id;
        public string ResultTower_Id;
        public int MergeCost;
    }

    [System.Serializable]
    public class MergeRecipeDatabase
    {
        public List<MergeRecipeData> Recipes;
    }
}