using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.Tilemaps.Tilemap;

namespace Adjustments
{
    public class Brand : IExposable
    {
        public Color Color;
        public string IconName;
        private Texture2D _colorTexture;
        public Texture2D ColorTexture
        {
            get
            {
                if (this._colorTexture == null)
                {
                    this._colorTexture = SolidColorMaterials.NewSolidColorTexture(Color);
                }
                return this._colorTexture;
            }
        }

        private Texture2D _icon;
        public Texture2D Icon { get
            {
                if (_icon==null)
                {
                    _icon= ContentFinder<Texture2D>.Get("adj/" + IconName);
                }
                return _icon;
            } }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Color, "adj-color");
            Scribe_Values.Look(ref IconName, "adj-icon-name");
        }
    }
    public class BrandComp : ThingComp
    {
        public static BrandComp Comp(Pawn pawn)
        {
            return pawn.GetComp<BrandComp>();
        }
        private List<Brand> _brands=new List<Brand>();
        public List<Brand> Brands { get
            {
                return _brands;
            } }

        private Pawn pawn;
        

        private Pawn Pawn => parent is Pawn pawn ? pawn : null;


        private TickManager _tickManager;
        public bool Paused { get {
                if (_tickManager == null)
                    _tickManager = Find.TickManager;
                return _tickManager.Paused;
        } }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(ref _brands, "nim-adjust-brands");

            if (_brands == null)
                _brands = new List<Brand>();
        }
        public void RemoveBrand(Brand brand)
        {
            _brands.Remove(brand);
            ManagerBrand.Invalidate = true;
        }
        public void AddBrand(Color c, string iconName)
        {
            _brands.Add(new Brand
            {
                Color = c,
                IconName = iconName
            });
            ManagerBrand.Invalidate = true;
        }

        public static void ShowDialog(Pawn pawn)
        {
            BrandDialog.Show(pawn);
        }

        public override void DrawGUIOverlay()
        {

            if (!Paused)
                return;

            var i = 0;
            foreach(var brand in Brands)
            {

                var vect = parent.DrawPos;
                //vect.x += .1f;
                vect.z += 1f;

                //var worldPost = new Vector2(vect.x, vect.z);
                //var text = brand.Label;
                //var textColor = brand.Color;

                Vector3 vector3 = vect; //new Vector3(worldPos.x, 0f, worldPos.y);
                Vector2 screenPoint = Find.Camera.WorldToScreenPoint(vector3) / Prefs.UIScale;
                screenPoint.y = (float)UI.screenHeight - screenPoint.y;

                //Text.Font = GameFont.Small;
                //GUI.color = textColor;
                //Text.Anchor = TextAnchor.UpperCenter;
                //float single = Text.CalcSize(text).x;

                screenPoint.y -= 21f * i;

                var rect = new Rect(screenPoint.x-10, screenPoint.y, 20f, 20f);
                //Widgets.Label(new Rect(screenPoint.x - single / 2f, screenPoint.y - 2f, single, 999f), text);
                //GUI.color = Color.white;
                //Text.Anchor = TextAnchor.UpperLeft;

                GUI.DrawTexture(rect, brand.Icon, ScaleMode.StretchToFill, true, 1f, brand.Color, 0f, 0f);

                //Widgets.ColorSelectorIcon(rect, brand.Color,);

                i++;
            }
            
        }



    }


}
