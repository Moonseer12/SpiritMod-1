using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SpiritMod.Items.Sets.GamblerChestLoot.GamblerChestNPCs;
using Microsoft.Xna.Framework;

namespace SpiritMod.Items.Sets.GamblerChestLoot
{
	public class CopperChest : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Copper Chest");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}\n'May contain a fortune'");
		}

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.buyPrice(silver: 10);
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 30;
			Item.autoReuse = true;
		}

		public override bool CanRightClick() => true;

		public override void RightClick(Player player)
		{
			int npcType = ModContent.NPCType<CopperChestBottom>();
			Vector2 position = player.Center + (Vector2.UnitX * player.direction * 30);

			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				NPC.NewNPC(player.GetSource_OpenItem(Item.type, "RightClick"), (int)position.X, (int)position.Y, npcType);
			}
			else if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer)
			{
				ModPacket packet = SpiritMod.Instance.GetPacket(MessageType.SpawnNPCFromClient, 3);
				packet.Write(npcType);
				packet.Write((int)position.X);
				packet.Write((int)position.Y);
				packet.Send();
			}
		}
	}
}
