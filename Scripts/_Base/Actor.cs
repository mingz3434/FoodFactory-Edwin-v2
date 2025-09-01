using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Actor : NetworkBehaviour { }

public class Actor_Game : Actor { }

public class Character_Game : Actor_Game { }

public class Machine : Actor_Game { }
