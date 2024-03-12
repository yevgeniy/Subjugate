using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Subjugate
{
    public  class Dialog : Window
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

        public Dialog(Pawn pawn, Action<List<string>> onSubmit)
        {
            this._pawn = pawn;
            this._onSubmit = onSubmit;
            SelectedIcon = "";
        }

        private CompSubjugate _comp;
        private CompSubjugate Comp
        {
            get
            {
                if (_comp == null)
                    _comp = CompSubjugate.GetComp(_pawn);
                return _comp;
            }
        }
        private Pawn Pawn { get { return _pawn; } }

        public string SelectedIcon { get; private set; }

        private void DoBrandRow(Rect rect)
        {

            //Widgets.BeginGroup(rect);

            //var iconRect = new Rect(0f, 0f, 30f, 30f);
            //GUI.DrawTexture(iconRect, brand.Icon, ScaleMode.StretchToFill, true, 1f, brand.Color, 0f, 0f);

            ////WidgetRow widgetRow = new WidgetRow(0f, 0f, UIDirection.RightThenUp, 99999f, 4f);
            ////widgetRow.Icon(brand.Icon);
            ////widgetRow.Gap(4f);

            //var buttonRect = new Rect(40f, 4f, 22f, 22f);

            //if (Widgets.ButtonImage(buttonRect, TexButton.DeleteX))
            //{
            //    Comp.RemoveBrand(brand);
            //}
            //Widgets.EndGroup();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            var top = 5f;

            RenderCurrentLevel(inRect, ref top);
            RenderXpDistribution(inRect, ref top);
            RenderPerkSelection(inRect, ref top);
            RenderSelectedPerks(inRect, ref top);



            //var colorPickerRect = new Rect(inRect.x + padding, top, inRect.width - padding * 2, 30f);
            //float h;
            //Widgets.ColorSelector(colorPickerRect, ref SelectedColor, Colors, out h);

            //top += 40f;

            //var iconRect = new Rect(padding, top, inRect.width, 60f);
            //Widgets.BeginGroup(iconRect);
            //var row = new WidgetRow(0f, 0f, UIDirection.RightThenDown, inRect.width - padding * 2);
            //foreach (var i in IconFiles)
            //{
            //    var tx = ContentFinder<Texture2D>.Get("adj/" + i);
            //    if (row.ButtonIcon(tx, null, null, Color.gray, Color.black))
            //        SelectedIcon = i;


            //}
            //Widgets.EndGroup();

            //top += 70f;

            //var buttonRect = new Rect(padding, top, 100f, 40f);
            //if (Widgets.ButtonText(buttonRect, "Add"))
            //{
            //    //Comp.AddBrand(SelectedColor, SelectedIcon);
            //}

            //top += 60f;

            //var listingRect = new Rect(5f, top, inRect.width, 400f);
            //Listing_Standard listingStandard = new Listing_Standard()
            //{
            //    ColumnWidth = inRect.width
            //};


            ////listingStandard.Begin(listingRect);
            ////int num = 0;
            ////for (int i = 0; i < Comp.Brands.Count; i++)
            ////{
            ////    var brand = Comp.Brands[i];

            ////    DoBrandRow(listingStandard.GetRect(24f, 1f), brand);
            ////    listingStandard.Gap(6f);
            ////    num++;

            ////}
            ////while (num < 9)
            ////{
            ////    listingStandard.Gap(30f);
            ////    num++;
            ////}
            //listingStandard.End();

        }

        private void RenderCurrentLevel(Rect inRect, ref float top)
        {
            var size = Text.CalcSize("Subjugation Level: " + Comp.Level);

            var textDiv = new Rect(inRect.width - size.x, top, size.x, size.y);
            Widgets.Label(textDiv, "Subjugation Level: " + Comp.Level);

            top += size.y + 3;

            Widgets.DrawLineHorizontal(0, top, inRect.width);

            top += 10;
        }

        private void RenderSelectedPerks(Rect inRect, ref float top)
        {

        }

        private void RenderPerkSelection(Rect inRect, ref float top)
        {
            GUI.color = new Color(0.3098039f, 0.3098039f, 0.3098039f, 1);
            Widgets.DrawLineHorizontal(0, top, inRect.width);
            GUI.color = Color.white;

            top += 10;

            var text = "Perks available:   " + GetAvailablePerks();
            var labeldiv = new Rect(0, top+2, 200, 30);

            Widgets.Label(labeldiv, text);

            var levelupdiv = new Rect(inRect.width - 200, top, 200, 30);
            if (Widgets.ButtonText(levelupdiv, "Select Perk", true, false, true))
            {
                DialogPerks.Show(Pawn);
            }
        }

        private string GetAvailablePerks()
        {
            return "4";
        }

        public static string Selected = "-- Select --";
        private void RenderXpDistribution(Rect inRect, ref float top)
        {
            GUI.color = new Color(0.6862745f, 0.6862745f, 0.6862745f, 1);
            Text.Font = GameFont.Tiny;

            var text = "XP committed to depricated skills can be distributed to a selected skill.  The rate of distribution " +
                "is current rate gain multiplied by subjugation level.  Keep in mind that xp in depricated skill will deteriorate " +
                "normally.";
            var textheight = Text.CalcHeight(text, inRect.width);

            var helperDiv = new Rect(0, top, inRect.width, textheight);
            Widgets.Label(helperDiv, text);

            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            top += textheight + 10;
            var dropdownDiv = new Rect(0, top, 100, 30);

            string[] options = GetSkills();
            if (Widgets.ButtonText(dropdownDiv, GetSelectedSkill(), true, false, true))
            {
                List<FloatMenuOption> list = options.Select(v => new FloatMenuOption(v, () => SetSelectedSkill(v) )).ToList();
                Find.WindowStack.Add(new FloatMenu(list));
            }


            Text.Font = GameFont.Tiny;
            var bufferXP = GetBufferXP();
            text = "XP to distribute:      " + bufferXP;
            var textSize = Text.CalcSize(text);
            var bufferXpDiv = new Rect(inRect.width - textSize.x, top+3, textSize.x, textSize.y);
            Widgets.Label(bufferXpDiv, text);

            Text.Font = GameFont.Small;

            top += 40;
        }

        private string GetBufferXP()
        {
            return "99999999";
        }

        private static string SetSelectedSkill(string v)
        {
            return Selected = v;
        }

        private static string GetSelectedSkill()
        {
            return Selected;
        }

        private static string[] GetSkills()
        {
            return new string[] {
                SkillDefOf.Plants.defName,
                SkillDefOf.Cooking.defName,
                SkillDefOf.Crafting.defName,
                SkillDefOf.Artistic.defName,
            };
        }

        public static Task<List<string>> Show(Pawn pawn)
        {
            var t = new TaskCompletionSource<List<string>>();
            Find.WindowStack.Add(new Dialog(pawn, onSubmit: (List<string> brands) =>
            {
                t.TrySetResult(brands);
            }));

            return t.Task;

        }
    }
}
