using System;

[Serializable]
public class Weapon
{
    public string id;
    public int damage;
    public float cooldown;

    public Weapon() { }
    public Weapon(string id, int damage, float cooldown) => (this.id, this.damage, this.cooldown) = (id, damage, cooldown);
    public override string ToString() => $"Weapon({id}, dmg:{damage}, cd:{cooldown})";
}