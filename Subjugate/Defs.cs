using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate
{
    [DefOf]
    public static class Defs
    {
        public static TraitDef Subj_ForTheLadies_Trait;

        public static ThoughtDef Subj_UnsubjugatedWomen_Thought;
        public static ThoughtDef Subj_AllWomenSlaves_Thought;
        public static ThoughtDef Subj_PussyShockRodInMyPussy_Thought;
        public static ThoughtDef Subj_GotPunished_Thought;
        public static ThoughtDef Subj_Binders_Thought;
        

        public static PreceptDef Subj_SubjugateAllWomen_Precept;

        public static ThingDef Subj_PussyShockRod_Item;
        public static ThingDef Subj_Bindings_Item;
        public static ThingDef Subj_ArmorVest_Item;

        public static ThingDef Subj_SlaveDress_Item;
        public static ThingDef Subj_ZDress_Item;
        public static ThingDef Subj_MaidDress_Item;
        public static ThingDef Subj_NippleRings_Item;
        public static ThingDef Subj_NippleBarbells_Item;
        public static ThingDef Subj_HeavyNippleRings_Item;
        public static ThingDef Subj_NippleChain_Item;
        public static ThingDef Subj_RingedNippleChain_Item;
        public static ThingDef Subj_TransparentSkirt_Item;
        

        public static HediffDef Subj_PussyShockRod_Hediff;
        public static HediffDef Subj_ShockTheGirl_Hediff;
        public static HediffDef Subj_PunishTheGirl_Hediff;
        public static HediffDef Subj_Subjugation_Hediff;
        public static HediffDef Subj_Bindings_Hediff;

        public static FleckDef Subj_NoHeart_Fleck;


        public static ThingCategoryDef Subj_Subjugation_ThingCategory;
        public static ThingCategoryDef Subj_SlaveGirl_ThingCategory;
        public static ThingCategoryDef Subj_Girl_ThingCategory;
        

        /*imported*/
        public static DamageDef VWE_ConditionalStun;
    }

    [StaticConstructorOnStartup]
    public static class find_defs
    {
        static find_defs()
        {
            Defs.VWE_ConditionalStun = DefDatabase<DamageDef>.AllDefs.FirstOrDefault(v => v.defName == "VWE_ConditionalStun");
        }
    }
}

