using RimWorld;
using Subjugate.SubjucationPerks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Subjugate
{
    public  class DialogPerks : Window
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

        public DialogPerks(Pawn pawn, Action<List<string>> onSubmit)
        {
            this._pawn = pawn;
            this._onSubmit = onSubmit;
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

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            var top = 5f;

            List<Perk> perks = GetApplicablePerks();

            foreach (Perk perk in perks)
            {
                RenderPerkSelection(inRect, perk, ref top);
                top += 15;
            }

        }

        private void RenderPerkSelection(Rect inRect, Perk perk, ref float top)
        {
            var t = top;
            
            var contentRect = inRect.ContractedBy(10f);
            t = top+10;

            Text.Font = GameFont.Small;
            GUI.color = new Color(0.937f, 0.937f, 0.937f, 1f);
            var banchor = Text.Anchor;

            /* label */
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Medium;
            var labeldiv = new Rect(contentRect.x, t, contentRect.width-125, 30);
            Widgets.Label(labeldiv, perk.Name);

            /* activate button */
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            var buttondiv = new Rect(inRect.width - 125, top, 125, 30);
            if (Widgets.ButtonText(buttondiv, "Activate", true, false, true))
            {

            }

            t += 40f;

            /* divider */
            GUI.color = new Color(0.535f, 0.535f, 0.535f, 1f);
            Widgets.DrawLineHorizontal(contentRect.x, t, contentRect.width);

            t += 10;

            /* Next level text */
            GUI.color = new Color(0.937f, 0.937f, 0.937f, 1f);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            var text = perk.NextLevelExplain(Pawn);
            var textheight = Text.CalcHeight(text, contentRect.width);
            var textdiv = new Rect(contentRect.x, t, contentRect.width, textheight);

            Widgets.Label(textdiv, text);
            t += textheight + 10;

            /* drawbox */
            GUI.color = new Color(0.535f, 0.535f, 0.535f, 1f);
            var boxdiv = new Rect(0, top, inRect.width, t - top);
            Widgets.DrawBox(boxdiv);

            top = t;
            Text.Anchor = banchor;
        }

        private List<Perk> GetApplicablePerks()
        {
            return new List<Perk>
            {
                new PerkArtistic(),
                new PerkNudistTrait()
            };
        }

        public static Task<List<string>> Show(Pawn pawn)
        {
            var t = new TaskCompletionSource<List<string>>();
            Find.WindowStack.Add(new DialogPerks(pawn, onSubmit: (List<string> brands) =>
            {
                t.TrySetResult(brands);
            }));

            return t.Task;

        }
    }
}
