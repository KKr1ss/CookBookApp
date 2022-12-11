﻿using CookBookApp.Helpers;
using CookBookApp.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookBookApp.Models
{
    public class Recipe : ICloneable
    {
        public int ID { get; set; }
        public int DefaultLanguageID { get; set; }
        public string Author { get; set; }
        public DateTime PreparationTime { get; set; }
        public int Difficulty { get; set; }
        public int Price { get; set; }
        public int Portion { get; set; }
        public DateTime CreationDate { get; set; }

        //tárolja a recept lokalizált nyelveit
        [NotMapped]
        public List<Language> Languages { get; set; }

        //tárolja a recepthez lekérdezett lokalizált receptjét
        [NotMapped]
        public RecipeLocalization LocalizedRecipe{ get; set; }

        //tárolja a recept lokalizációit
        [NotMapped]
        public List<RecipeLocalization> Localizations { get; set; }

        //tárolja a recept categóriáit
        [NotMapped]
        public List<RecipeCategories> Categories { get; set; }

        //tárolja a recept képeit
        [NotMapped]
        public List<RecipeImage> Images { get; set; }

        [NotMapped]
        public string LocalizedDifficulty
        {
            get
            {
                string[] difficultes = DifficultyHelper.getDifficulties();
                return difficultes[Difficulty];
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}