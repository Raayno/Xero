using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This helper class, meant to be used by the MMAdditiveSceneLoadingManager, creates a temporary scene to store objects that might get instantiated, and empties it in the destination scene once loading is complete
	/// </summary>
	public class MMSceneLoadingAntiSpill
	{
		protected Scene _antiSpillScene;
		protected Scene _destinationScene;
		protected UnityAction<Scene, Scene> _onActiveSceneChangedCallback;
		protected string _sceneToLoadName;
		protected string _antiSpillSceneName;
		protected AsyncOperationHandle<SceneInstance> _antiSpillHandle;
		protected List<GameObject> _spillSceneRoots = new List<GameObject>(50);
		protected static List<string> _scenesInBuild;
		
		/// <summary>
		/// Creates the temporary scene
		/// </summary>
		/// <param name="sceneToLoadName"></param>
		public virtual void PrepareAntiFill(string sceneToLoadName, string antiSpillSceneName = "")
		{			
			var sourceSkybox = RenderSettings.skybox;
			var sourceAmbientMode = RenderSettings.ambientMode;
			var sourceAmbientLight = RenderSettings.ambientLight;
			var sourceAmbientSkyColor = RenderSettings.ambientSkyColor;
			var sourceAmbientEquatorColor = RenderSettings.ambientEquatorColor;
			var sourceAmbientGroundColor = RenderSettings.ambientGroundColor;
			var sourceFog = RenderSettings.fog;
			var sourceFogColor = RenderSettings.fogColor;
			var sourceFogMode = RenderSettings.fogMode;
			var sourceFogDensity = RenderSettings.fogDensity;
			var sourceFogStartDistance = RenderSettings.fogStartDistance;
			var sourceFogEndDistance = RenderSettings.fogEndDistance;
			var sourceLightmapsMode = LightmapSettings.lightmapsMode;
			var sourceLightProbes = LightmapSettings.lightProbes;
			var sourceLightmaps = LightmapSettings.lightmaps;
			
			_destinationScene = default; 
			_sceneToLoadName = sceneToLoadName;
			
			if (antiSpillSceneName == "")
			{
				_antiSpillScene = SceneManager.CreateScene($"AntiSpill_{sceneToLoadName}");

				PrepareAntiFillSetSceneActive();
			}
			else
			{
				_antiSpillHandle = Addressables.LoadSceneAsync(antiSpillSceneName, LoadSceneMode.Additive, true);
				_antiSpillHandle.WaitForCompletion();
				_antiSpillScene = _antiSpillHandle.Result.Scene;
				_antiSpillSceneName = _antiSpillScene.name;
				PrepareAntiFillSetSceneActive();
			}
			
			RenderSettings.skybox = sourceSkybox;
			RenderSettings.ambientMode = sourceAmbientMode;
			RenderSettings.ambientLight = sourceAmbientLight;
			RenderSettings.ambientSkyColor = sourceAmbientSkyColor;
			RenderSettings.ambientEquatorColor = sourceAmbientEquatorColor;
			RenderSettings.ambientGroundColor = sourceAmbientGroundColor;
			RenderSettings.fog = sourceFog;
			RenderSettings.fogColor = sourceFogColor;
			RenderSettings.fogMode = sourceFogMode;
			RenderSettings.fogDensity = sourceFogDensity;
			RenderSettings.fogStartDistance = sourceFogStartDistance;
			RenderSettings.fogEndDistance = sourceFogEndDistance;
			LightmapSettings.lightmapsMode = sourceLightmapsMode;
			LightmapSettings.lightProbes = sourceLightProbes;
			LightmapSettings.lightmaps = sourceLightmaps;
		}

		/// <summary>
		/// Sets the anti spill scene active
		/// </summary>
		protected virtual void PrepareAntiFillSetSceneActive()
		{
			if (_onActiveSceneChangedCallback != null) { SceneManager.activeSceneChanged -= _onActiveSceneChangedCallback; }
			_onActiveSceneChangedCallback = OnActiveSceneChanged;
			SceneManager.activeSceneChanged += _onActiveSceneChangedCallback;
			SceneManager.SetActiveScene(_antiSpillScene);
		}
		
		/// <summary>
		/// Once the destination scene has been loaded, we catch that event and prepare to empty
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		protected virtual void OnActiveSceneChanged(Scene from, Scene to)
		{
			if (from == _antiSpillScene)
			{
				SceneManager.activeSceneChanged -= _onActiveSceneChangedCallback;
				_onActiveSceneChangedCallback = null;
				
				EmptyAntiSpillScene();
			}
		}

		/// <summary>
		/// Empties the contents of the anti spill scene into the destination scene
		/// </summary>
		protected virtual void EmptyAntiSpillScene()
		{
			if (_antiSpillScene.IsValid() && _antiSpillScene.isLoaded)
			{
				_spillSceneRoots.Clear();
				_antiSpillScene.GetRootGameObjects(_spillSceneRoots);

				_destinationScene = SceneManager.GetSceneByName(_sceneToLoadName);
				
				if (_spillSceneRoots.Count > 0)
				{
					if (_destinationScene.IsValid() && _destinationScene.isLoaded)
					{
						foreach (var root in _spillSceneRoots)
						{
							SceneManager.MoveGameObjectToScene(root, _destinationScene);
						}
					}
				}

				if (!string.IsNullOrEmpty(_antiSpillSceneName))
				{
					Addressables.UnloadSceneAsync(_antiSpillHandle, true);
				}
				else
				{
					SceneManager.UnloadSceneAsync(_antiSpillScene);
				}
			}
		}
	}
}