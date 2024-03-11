using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Adjustments
{
    public class ManagerBrand : MapComponent
    {
        public ManagerBrand(Map map) : base(map)
        {
        }

        private TickManager _tickManager;
        public bool Paused
        {
            get
            {
                if (_tickManager == null)
                    _tickManager = Find.TickManager;
                return _tickManager.Paused;
            }
        }
        public static bool Invalidate = false;
        private List<Brand> _brands;
        private List<Brand> Brands
        {
            get
            {
                if (Invalidate || _brands==null)
                {
                    var pawns = Find.CurrentMap.mapPawns.AllPawns;

                    _brands = pawns.Select(v => v.GetComp<BrandComp>()).SelectMany(v => v.Brands).ToList();

                    Invalidate = false;
                }
                return _brands;
            }
        }
        private List<Pawn> SpawnedColonyMechs()
        {
            var mapPawns = Find.CurrentMap.mapPawns;

            List<Pawn> pawns = new List<Pawn>();
            foreach (Pawn pawn in mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
            {
                if (pawn.IsColonyMech)
                {
                    pawns.Add(pawn);
                }
                else if (pawn.IsColonyMechPlayerControlled)
                {
                    pawns.Add(pawn);
                }
            }
            return pawns;
        }

        public override void MapComponentOnGUI()
        {
            base.MapComponentOnGUI();

            if (Paused)
            {                
                var dict = new Dictionary<Brand, int>();
                foreach (var brand in Brands)
                {
                    var existingkey = dict.Keys.FirstOrDefault(v => v.Color == brand.Color && v.IconName == brand.IconName);
                    if (existingkey==null)
                    {
                        dict.Add(brand, 0);
                        existingkey = brand;
                    }
                    dict[existingkey]++;
                }

                var top = 0f;
                var screenWidth = Screen.width;
                var cellWidth = 25f;
                Text.Font = GameFont.Small;
                foreach (var entry in dict)
                {

                    var iconRect = new Rect(screenWidth - cellWidth * 2, top, cellWidth, cellWidth);
                    GUI.DrawTexture(iconRect, entry.Key.Icon, ScaleMode.ScaleToFit, true, 1f, entry.Key.Color, 0f, 0f);

                    var textRect = new Rect(screenWidth - cellWidth, top, cellWidth, cellWidth);
                    Widgets.TextArea(textRect, entry.Value.ToString(), true);

                    top += 30f;
                }

            } else
                Invalidate = true;

            
        }
        
    }
}
