using Microsoft.Xna.Framework.Graphics;
using SpiritMod.Biomes;
using SpiritMod.Items.Glyphs;
using SpiritMod.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static SpiritMod.NPCUtils;
using Terraria.GameContent.Bestiary;

namespace SpiritMod.NPCs.Town
{
	[AutoloadHead]
	public class RuneWizard : ModNPC
	{
		public override string Texture => "SpiritMod/NPCs/Town/RuneWizard";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Enchanter");
			Main.npcFrameCount[NPC.type] = 26;
			NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
			NPCID.Sets.AttackFrameCount[NPC.type] = 4;
			NPCID.Sets.DangerDetectRange[NPC.type] = 1500;
			NPCID.Sets.AttackType[NPC.type] = 0;
			NPCID.Sets.AttackTime[NPC.type] = 25;
			NPCID.Sets.AttackAverageChance[NPC.type] = 30;

			NPC.Happiness
				.SetBiomeAffection<SpiritSurfaceBiome>(AffectionLevel.Like).SetBiomeAffection<SpiritUndergroundBiome>(AffectionLevel.Like)
				.SetBiomeAffection<UndergroundBiome>(AffectionLevel.Like)
				.SetBiomeAffection<HallowBiome>(AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.GoblinTinkerer, AffectionLevel.Love)
				.SetNPCAffection(NPCID.Demolitionist, AffectionLevel.Like)
				.SetNPCAffection(NPCID.Wizard, AffectionLevel.Hate);
		}

		public override void SetDefaults()
		{
			NPC.CloneDefaults(NPCID.Guide);
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.aiStyle = 7;
			NPC.damage = 14;
			NPC.defense = 30;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.4f;
			AnimationType = NPCID.Guide;
			SpawnModBiomes = new int[1] { ModContent.GetInstance<SpiritSurfaceBiome>().Type };
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				new FlavorTextBestiaryInfoElement("This tired rune scribe has spent sleepless nights studying the ancient magic found in glyphs. He's not much of a conversationalist, but he'll gladly empower your arsenal."),
			});
		}

		public override bool CanTownNPCSpawn(int numTownNPCs, int money) => Main.player.Any(x => x.active && x.inventory.Any(y => y.type == ModContent.ItemType<Glyph>()));

		public override List<string> SetNPCNameList() => new() { "Malachai", "Nisarmah", "Moneque", "Tosalah", "Kentremah", "Salqueeh", "Oarno", "Cosimo" };

		public override string GetChat()
		{
			List<string> dialogue = new List<string>
			{
				"Power up your weapons with my strange Glyphs!",
				"Sometimes, I just scribble a rune on a Glyph and see what happens. I don't recommend you do.",
				"Before you ask, no, I'm not going to put a Honeyed Glyph on a bee. It'd be way too strong.",
				"I forgot the essence of Hellebore! Don't touch that!",
				"If you're unsure of how to stumble upon Glyphs, my master once told me powerful bosses hold many!",
				"Fun fact - you can put runes on anything. They're just most powerful on Glyphs.",
				"Anything can be enchanted if you possess the skill, wit, and essence!",
			};

			dialogue.AddWithCondition("I wonder what enchantements have been placed on the moon - It's all blue!", MyWorld.blueMoon);
			dialogue.AddWithCondition("The resurgence of Spirits offer a whole level of enchanting possibility!", Main.hardMode);

			return Main.rand.Next(dialogue);
		}

		public override void SetChatButtons(ref string button, ref string button2) => button = Language.GetTextValue("LegacyInterface.28");

		public override void OnChatButtonClicked(bool firstButton, ref bool shop)
		{
			if (firstButton)
				shop = true;
		}

		public override void SetupShop(Chest shop, ref int nextSlot)
		{
			AddItem(ref shop, ref nextSlot, ModContent.ItemType<NullGlyph>());

			CustomWare(shop.item[nextSlot++], ModContent.ItemType<FrostGlyph>());
			CustomWare(shop.item[nextSlot++], ModContent.ItemType<EfficiencyGlyph>());

			if (NPC.downedBoss1)
			{
				CustomWare(shop.item[nextSlot++], ModContent.ItemType<RadiantGlyph>());
				CustomWare(shop.item[nextSlot++], ModContent.ItemType<SanguineGlyph>(), 3);
			}
			if (MyWorld.downedReachBoss)
				CustomWare(shop.item[nextSlot++], ModContent.ItemType<StormGlyph>(), 2);
			if (NPC.downedBoss2)
				CustomWare(shop.item[nextSlot++], ModContent.ItemType<UnholyGlyph>(), 2);
			if (NPC.downedBoss3)
				CustomWare(shop.item[nextSlot++], ModContent.ItemType<VeilGlyph>(), 3);
			if (NPC.downedQueenBee)
				CustomWare(shop.item[nextSlot++], ModContent.ItemType<BeeGlyph>(), 3);
			if (Main.hardMode)
				CustomWare(shop.item[nextSlot++], ModContent.ItemType<BlazeGlyph>(), 3);
			if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
				CustomWare(shop.item[nextSlot++], ModContent.ItemType<VoidGlyph>(), 4);
			if (MyWorld.downedDusking)
				CustomWare(shop.item[nextSlot++], ModContent.ItemType<PhaseGlyph>(), 4);

			AddItem(ref shop, ref nextSlot, ModContent.ItemType<Items.Armor.WitchSet.WitchHead>(), 12000, !Main.dayTime);
			AddItem(ref shop, ref nextSlot, ModContent.ItemType<Items.Armor.WitchSet.WitchBody>(), 15000, !Main.dayTime);
			AddItem(ref shop, ref nextSlot, ModContent.ItemType<Items.Armor.WitchSet.WitchLegs>(), 10000, !Main.dayTime);
		}

		private static void CustomWare(Item item, int type, int price = 1)
		{
			item.SetDefaults(type);
			item.shopCustomPrice = price;
			item.shopSpecialCurrency = SpiritMod.GlyphCurrencyID;
		}


		public override void TownNPCAttackStrength(ref int damage, ref float knockback)
		{
			damage = 18;
			knockback = 3f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
		{
			cooldown = 5;
			randExtraCooldown = 5;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
		{
			projType = ProjectileID.RubyBolt;
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
		{
			multiplier = 14f;
			randomOffset = 2f;
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			if (NPC.life > 0 || Main.netMode == NetmodeID.Server)
				return;

			for (int i = 0; i < 4; i++)
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("Enchanter" + (i + 1)).Type);
		}

		private float animCounter;
		public override void FindFrame(int frameHeight)
		{
			if (!NPC.IsABestiaryIconDummy)
				return;

			animCounter += 0.25f;
			if (animCounter >= 16)
				animCounter = 2;
			else if (animCounter < 2)
				animCounter = 2;

			int frame = (int)animCounter;
			NPC.frame.Y = frame * frameHeight;
		}

		public override ITownNPCProfile TownNPCProfile() => new RuneWizardProfile();
	}

	public class RuneWizardProfile : ITownNPCProfile
	{
		public int RollVariation() => 0;
		public string GetNameForVariant(NPC npc) => npc.getNewNPCName();

		public ReLogic.Content.Asset<Texture2D> GetTextureNPCShouldUse(NPC npc)
		{
			if (npc.altTexture == 1 && !(npc.IsABestiaryIconDummy && !npc.ForcePartyHatOn))
				return ModContent.Request<Texture2D>("SpiritMod/NPCs/Town/RuneWizard_Alt_1");

			return ModContent.Request<Texture2D>("SpiritMod/NPCs/Town/RuneWizard");
		}

		public int GetHeadTextureIndex(NPC npc) => ModContent.GetModHeadSlot("SpiritMod/NPCs/Town/RuneWizard_Head");
	}
}