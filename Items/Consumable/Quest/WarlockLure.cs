using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using SpiritMod.Mechanics.QuestSystem;
using SpiritMod.Mechanics.QuestSystem.Quests;
using SpiritMod.Mechanics.QuestSystem.Tasks;

namespace SpiritMod.Items.Consumable.Quest
{
	[Sacrifice(1)]
	public class WarlockLureCorruption : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Unholy Magic");
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(8, 7));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        }

		public override void SetDefaults()
		{
			Item.width = Item.height = 16;
			Item.rare = ItemRarityID.Green;
			Item.maxStack = 99;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if (!QuestManager.GetQuest<ZombieOriginQuest>().IsCompleted)
			{
				TooltipLine line = new TooltipLine(Mod, "ItemName", "Quest Item");
				line.OverrideColor = new Color(100, 222, 122);
				tooltips.Add(line);
				TooltipLine line1 = new TooltipLine(Mod, "FavoriteDesc", "'Hopefully, the evil necromancer is intrigued by this strange magic'");
				line1.OverrideColor = new Color(255, 255, 255);
				tooltips.Add(line1);
			}
			else
			{
				TooltipLine line1 = new TooltipLine(Mod, "FavoriteDesc", "The Undead Warlock is no more!\nThere's no need for this repulsive thing anymore");
				line1.OverrideColor = new Color(255, 255, 255);
				tooltips.Add(line1);
			}
		}

		public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200);

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddCondition(QuestCondition(Type));
			recipe.AddIngredient(ItemID.FallenStar, 2);
			recipe.AddIngredient(ItemID.RottenChunk, 5);
			recipe.Register();
		}

		public static Recipe.Condition QuestCondition(int type) => new Recipe.Condition(Terraria.Localization.NetworkText.FromLiteral("During Unholy Undertaking"), (self) =>
		{
			Mechanics.QuestSystem.Quest quest = QuestManager.GetQuest<ZombieOriginQuest>();
			return (quest.CurrentTask is RetrievalTask task && task.GetItemID() == type);
		});
	}

    public class WarlockLureCrimson : WarlockLureCorruption
    {
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddCondition(QuestCondition(Type));
			recipe.AddIngredient(ItemID.FallenStar, 2);
			recipe.AddIngredient(ItemID.Vertebrae, 5);
			recipe.Register();
		}
	}
}
