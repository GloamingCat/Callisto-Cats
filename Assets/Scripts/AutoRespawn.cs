using UnityEngine;

public class AutoRespawn : MonoBehaviour {

    public SpawnSpot spot;

    private void OnDestroy() {
        if (spot != null)
            spot.TriggerRespawn();
    }

}
