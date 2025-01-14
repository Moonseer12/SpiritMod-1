﻿
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.NPCs.Spirit
{
	class SpiritScythe : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Soul Scythe");
			Main.projFrames[base.Projectile.type] = 6;
		}

		int timer = 0;
		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = 2;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 300;
			Projectile.height = 28;
			Projectile.width = 28;
			AIType = ProjectileID.Bullet;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			Projectile.rotation += 0.2f;

			int dust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Flare_Blue, Projectile.velocity.X, Projectile.velocity.Y);
			int dust2 = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Flare_Blue, Projectile.velocity.X, Projectile.velocity.Y);
			Main.dust[dust].noGravity = true;
			Main.dust[dust2].noGravity = true;
			Main.dust[dust2].velocity *= 0f;
			Main.dust[dust2].velocity *= 0f;
			Main.dust[dust2].scale = 1.2f;
			Main.dust[dust].scale = 1.2f;

			timer++;
			if (timer == 10)
				Projectile.velocity *= 0.1f;
			else if (timer == 20)
				Projectile.velocity *= 5f;
			else if (timer == 40)
				Projectile.velocity *= 5f;
			else if (timer == 300)
				timer = 0;

		}

		public override void Kill(int timeLeft)
		{
			int dust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Flare_Blue, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
			int dust2 = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Flare_Blue, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
			Main.dust[dust].noGravity = true;
			Main.dust[dust2].noGravity = true;
			Main.dust[dust2].velocity *= 0f;
			Main.dust[dust2].velocity *= 0f;
			Main.dust[dust2].scale = 1.2f;
			Main.dust[dust].scale = 1.2f;
		}

	}
}
