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

        private float SelectedPerkHeight;
        private Vector2 CurrentScrollForPerks;
        private void RenderSelectedPerks(Rect inRect, ref float top)
        {
            var va = Text.Anchor;
            GUI.color = new Color(0.3098039f, 0.3098039f, 0.3098039f, 1);
            Widgets.DrawLineHorizontal(0, top, inRect.width);

            top += 15;

            var scrollrect=new Rect(0, top, inRect.width, inRect.height-top);
            var scrollview = new Rect(0, 0, inRect.width - 20, SelectedPerkHeight);
            List<KeyValuePair<string, string>> selections = GetSelectedPerks();
            
            Widgets.BeginScrollView(scrollrect, ref CurrentScrollForPerks, scrollview, true);
            Text.Font = GameFont.Small;

            var lens = selections.Select(v => Text.CalcSize(v.Key).x);
            var len = lens.Max();
            var padding = 5f;
            var width = scrollview.width;
            var labelWidth = len;
            var labelSectionWidth = labelWidth + padding * 2;
            var descSectionWidth = width - labelSectionWidth;
            var descWidth = descSectionWidth - padding * 2;

            var t = 0f;

            foreach (var entry in selections)
            {
                Text.Font = GameFont.Tiny;
                var textheight = Text.CalcHeight(entry.Value, descWidth);
                textheight = Mathf.Max(30, textheight);

                /*draw box*/
                GUI.color = new Color(0.535f, 0.535f, 0.535f, 1f);
                var boxdiv = new Rect(0, t, width, textheight+padding*2);
                Widgets.DrawBox(boxdiv);

                /*draw label background*/
                var labelboxdiv=new Rect(0, t, labelSectionWidth, textheight+padding*2);
                Widgets.DrawBoxSolid(labelboxdiv, new Color(0.435f, 0.435f, 0.435f, 1f));

                t += padding;

                /*draw label*/
                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                var labeldiv = new Rect(padding, t, labelWidth, textheight);
                Widgets.Label(labeldiv, entry.Key);

                /*draw explain*/
                Text.Anchor= TextAnchor.MiddleLeft;
                Text.Font = GameFont.Tiny;
                var descdiv = new Rect(labelSectionWidth+padding, t, descWidth, textheight);
                Widgets.Label(descdiv, entry.Value);

                t += textheight + padding + 15;
            }
            SelectedPerkHeight = t;

            Widgets.EndScrollView();
            Text.Anchor = va;

            top += t + 10;
        }

        private List<KeyValuePair<string, string>> GetSelectedPerks()
        {
            var dict = new List<KeyValuePair<string,string>>();
            dict.Add(new KeyValuePair<string, string>("Artistic", "this lady will now do artistic worktypes"));
            dict.Add(new KeyValuePair<string, string>("Shak Spear", "To be or not to be that is the question.  To fly to worlds of unknown to shake off the shackles or oppression which this lifes throughs at you and by resisting this tyranny, end it.  But what does wait beyond the veil that is the question.  Conscience makes cowards of us all."));
            dict.Add(new KeyValuePair<string, string>("Nude", "this lady likes being naked"));
            dict.Add(new KeyValuePair<string, string>("Artistic", "this lady will now do artistic worktypes"));
            dict.Add(new KeyValuePair<string, string>("Artistic", "this lady will now do artistic worktypes"));
            dict.Add(new KeyValuePair<string, string>("Artistic", "this lady will now do artistic worktypes"));
            dict.Add(new KeyValuePair<string, string>("Artistic", "this lady will now do artistic worktypes"));
            dict.Add(new KeyValuePair<string, string>("Artistic", "this lady will now do artistic worktypes"));
            dict.Add(new KeyValuePair<string, string>("Artistic", "this lady will now do artistic worktypes"));
            dict.Add(new KeyValuePair<string, string>("Artistic", "this lady will now do artistic worktypes"));
            dict.Add(new KeyValuePair<string, string>("Artistic", "this lady will now do artistic worktypes"));
            dict.Add(new KeyValuePair<string, string>("Artistic", "this lady will now do artistic worktypes"));
            return dict;
            
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

            top += 40;
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

            string[] options = GetDepricatedSkills();
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
            return Comp.xp.XPBuffer.ToString("N");
        }

        private static string SetSelectedSkill(string v)
        {
            return Selected = v;
        }

        private static string GetSelectedSkill()
        {
            return Selected;
        }

        private static string[] GetDepricatedSkills()
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
