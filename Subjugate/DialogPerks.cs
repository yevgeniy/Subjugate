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


        public DialogPerks(Pawn pawn, Action<List<string>> onSubmit)
        {
            this._pawn = pawn;
        }

        private CompSubjugate _comp;
        private Vector2 scrollPosition;

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
        private float HeightMod;

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            var top = 5f;
            var viewRect = new Rect(0, 0, inRect.width - 15, HeightMod);

            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

            List<Perk> perks = GetApplicablePerks();

            foreach (Perk perk in perks)
            {
                RenderPerkCard(viewRect, perk, ref top);
                top += 15;
            }

            HeightMod = top;

            Widgets.EndScrollView();

        }

        private void RenderPerkCard(Rect inRect, Perk perk, ref float top)
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
                var type = perk.GetType();
                var newperk = (Perk)Activator.CreateInstance(type);
                Comp.AddPerk(newperk);
                Close();
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
            var skills= new List<Perk>
            {
                new PerkArtistic(),
                new PerkPlants(),
                new PerkCooking(),
                new PerkNudistTrait(),
                new PerkGainXPPool(),
                new PerkSubmissive(),
                new PerkSocial(),
                
            };

            return skills.Where(v => v.NextLevelExplain(Pawn) != null).ToList();
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
