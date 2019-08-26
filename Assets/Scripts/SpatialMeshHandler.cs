using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.Utilities;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialMeshHandler : MonoBehaviourPun, IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessMeshObject>
{
    private PhotonView RPCPhotonView;
    private Dictionary<int, int> meshDataUpdates;

    void Start()
    {
        RPCPhotonView = GetComponent<PhotonView>();
        meshDataUpdates = new Dictionary<int, int>();

    }

    //for accessing SpatialAwarenessSystem
    private IMixedRealitySpatialAwarenessSystem spatialAwarenessSystem = null;

    //for accessing DataProvider
    private IMixedRealityDataProviderAccess dataProviderAccess = null;

    //for accessing MeshObserver
    public IMixedRealitySpatialAwarenessMeshObserver meshObserver = null;

    public IMixedRealitySpatialAwarenessSystem SpatialAwarenessSystem
    {
        get
        {
            if (spatialAwarenessSystem == null)
            {
                MixedRealityServiceRegistry.TryGetService<IMixedRealitySpatialAwarenessSystem>(out spatialAwarenessSystem);
            }
            return spatialAwarenessSystem;
        }
    }

    public IMixedRealityDataProviderAccess DataProviderAccess
    {
        get
        {
            if (dataProviderAccess == null)
            {
                dataProviderAccess = spatialAwarenessSystem as IMixedRealityDataProviderAccess;
            }
            return dataProviderAccess;
        }
    }

    private IMixedRealitySpatialAwarenessMeshObserver MeshObserver
    {
        get
        {
            if (dataProviderAccess != null)
                meshObserver = dataProviderAccess.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
            else
                Debug.Log("DataAccessProvider is null");
            return meshObserver;
        }
       
    }

    //private Dictionary<int, uint> meshUpdateData = new Dictionary<int, uint>();

   

    private async void OnEnable()
    {
        await new WaitUntil(() => SpatialAwarenessSystem != null);
        SpatialAwarenessSystem.Register(gameObject);
        await new WaitUntil(() => DataProviderAccess != null);
        await new WaitUntil(() => MeshObserver != null);     
    }

    private void OnDisable()
    {
        SpatialAwarenessSystem?.Unregister(gameObject);
    }

  
    public void OnObservationAdded(MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> eventData)
    {
        if (!meshDataUpdates.ContainsKey(eventData.Id))
        {
            meshDataUpdates.Add(eventData.Id, 0);
        }


        /*
        if(!meshUpdateData.ContainsKey(eventData.Id))
        {
            Debug.Log($"Added mesh {eventData.Id} to observation");
            meshUpdateData.Add(eventData.Id, 0);

            var newVertices = eventData.SpatialObject.Filter.mesh.vertices;
            var newUVs = eventData.SpatialObject.Filter.mesh.uv;
            var newTriangles = eventData.SpatialObject.Filter.mesh.triangles;
            var meshPosition = eventData.SpatialObject.Filter.transform.position;
            var meshId = eventData.Id;


            //RPCPhotonView.RPC("RecieveMeshData", RpcTarget.Others, newVertices, newUVs, newTriangles, meshPosition, meshId);
            
        }*/
    }

    public void OnObservationRemoved(MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> eventData)
    {
        if(meshDataUpdates.ContainsKey(eventData.Id))
            {
            RPCPhotonView.RPC("RemoveMesh", RpcTarget.Others, eventData.Id);          
            meshDataUpdates.Remove(eventData.Id);
            }
    }

    public void OnObservationUpdated(MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> eventData)
    {
        int updateCount = 0;

        if(meshDataUpdates.TryGetValue(eventData.Id, out updateCount))
        {
            meshDataUpdates[eventData.Id] = ++updateCount;

            if ((updateCount % 6) == 0)
            {
                RPCPhotonView.RPC("RecieveMeshData", RpcTarget.Others, eventData.SpatialObject.Filter.mesh.vertices, eventData.SpatialObject.Filter.mesh.triangles,
                    eventData.SpatialObject.Filter.mesh.uv, eventData.SpatialObject.Id);
            }
        }

        /* var newVertices = eventData.SpatialObject.Filter.mesh.vertices;
         var newUVs = eventData.SpatialObject.Filter.mesh.uv;
         var newTriangles = eventData.SpatialObject.Filter.mesh.triangles;
         var meshId = eventData.SpatialObject.Id;

         RPCPhotonView.RPC("UpdateMesh", RpcTarget.Others, newVertices, newUVs, newTriangles, meshId); */
    }

    [PunRPC]
    public void SendMeshData()
    {

    }

}
