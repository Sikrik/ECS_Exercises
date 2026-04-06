public class PlayerTag : Component { }
public class EnemyTag : Component { }
public class BulletTag : Component { }
public class BouncyTag : Component { }      // 标记实体具有弹性碰撞行为
public class NeedsBakingTag : Component { } // 标记实体需要初始化物理数据
// 这是一个标记组件，没有任何数据，只代表“该实体等待回收”
public class DestroyTag : Component { }