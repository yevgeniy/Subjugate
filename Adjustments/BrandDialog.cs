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
    public class BrandDialog : Window
    {
        private Pawn _pawn;

        private List<string> IconFiles = new List<string>{
                "adj_alien", "adj_anim", "adj_blowup", "adj_build", "adj_cold", "adj_craft", "adj_evil", "adj_fight", "adj_fire", "adj_food", "adj_heart", "adj_lightning", "adj_magic", "adj_med", "adj_mine", "adj_peace", "adj_ranged", "adj_rebel", "adj_research", "adj_rocket", "adj_space", "adj_tec", "adj_virus", "Ideoligion_AnimalPrintC", "Ideoligion_AnimalPrintJ", "Ideoligion_GameI"
        };

        private readonly Action<List<string>> _onSubmit;
        private static Color SelectedColor;
        private static List<Color> Colors = new List<Color> { 
                 Color.cyan,
                Color.blue,
                 Color.gray,
                 Color.green,
                 Color.magenta,
                Color.red,
                Color.white,
                Color.yellow
        };

        public BrandDialog(Pawn pawn, Action<List<string>> onSubmit)
        {
            this._pawn = pawn;
            this._onSubmit = onSubmit;
            SelectedIcon = "";
        }
   
        private BrandComp _comp;
        private BrandComp Comp
        {
            get
            {
                if (_comp==null)
                    _comp = _pawn.GetComp<BrandComp>();
                return _comp;
            }
        }

        public string SelectedIcon { get; private set; }

        private void DoBrandRow(Rect rect, Brand brand)
        {
            
            Widgets.BeginGroup(rect);

            var iconRect = new Rect(0f, 0f, 30f, 30f);
            GUI.DrawTexture(iconRect, brand.Icon, ScaleMode.StretchToFill, true, 1f, brand.Color, 0f, 0f);

            //WidgetRow widgetRow = new WidgetRow(0f, 0f, UIDirection.RightThenUp, 99999f, 4f);
            //widgetRow.Icon(brand.Icon);
            //widgetRow.Gap(4f);

            var buttonRect = new Rect(40f, 4f, 22f, 22f);
            
            if (Widgets.ButtonImage(buttonRect, TexButton.DeleteX))
            {
                Comp.RemoveBrand(brand);
            }
            Widgets.EndGroup();
        }

        public override void DoWindowContents(Rect inRect)
        {
            var top = 5f;
            var padding = 3f;

            var colorPickerRect = new Rect(inRect.x+padding, top, inRect.width-padding*2, 30f);
            float h;
            Widgets.ColorSelector(colorPickerRect, ref SelectedColor, Colors, out h);

            top += 40f;

            var iconRect = new Rect(padding, top, inRect.width, 60f); 
            Widgets.BeginGroup(iconRect);
            var row = new WidgetRow(0f, 0f, UIDirection.RightThenDown, inRect.width-padding*2);
            foreach(var i in IconFiles)
            {
                var tx = ContentFinder<Texture2D>.Get("adj/" + i);
                if (row.ButtonIcon(tx, null, null, Color.gray, Color.black))
                    SelectedIcon = i;


            }
            Widgets.EndGroup();

            top += 70f;

            var buttonRect = new Rect(padding, top, 100f, 40f);
            if (Widgets.ButtonText(buttonRect, "Add"))
            {
                Comp.AddBrand(SelectedColor, SelectedIcon);
            }

            top += 60f;

            var listingRect = new Rect(5f, top, inRect.width, 400f);
            Listing_Standard listingStandard = new Listing_Standard()
            {
                ColumnWidth = inRect.width
            };


            listingStandard.Begin(listingRect);
            int num = 0;
            for (int i = 0; i < Comp.Brands.Count; i++)
            {
                var brand = Comp.Brands[i];

                DoBrandRow(listingStandard.GetRect(24f, 1f), brand);
                listingStandard.Gap(6f);
                num++;
                
            }
            while (num < 9)
            {
                listingStandard.Gap(30f);
                num++;
            }
            listingStandard.End();

        }

        public static Task<List<string>> Show(Pawn pawn )
        {
            var t = new TaskCompletionSource<List<string>>();
            Find.WindowStack.Add(new BrandDialog(pawn, onSubmit: (List<string> brands) =>
            {
                t.TrySetResult(brands);
            }));

            return t.Task;

        }
    }
}
