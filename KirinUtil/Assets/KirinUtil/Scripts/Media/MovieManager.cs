﻿using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace KirinUtil {
    public class MovieManager : MonoBehaviour {

        [Serializable]
        public class UIMovies {
            public string fileName = "";
            public GameObject obj;
            public bool visible;
            public bool loop;
            public float volume;

            public void Init() {
                fileName = "";
                obj = null;
                visible = false;
                loop = false;
                volume = 1.0f;
            }
        }

        public enum RootPath {
            dataPath,
            persistentDataPath,
            temporaryCachePath,
            streamingAssetsPath
        }
        [SerializeField] RootPath rootPath;
        public string movieDirPath = "/../../AppData/Data/Movies/";
        public UIMovies[] uiMovies;
        [NonSerialized] public List<MediaPlayer> mediaPlayer = new List<MediaPlayer>();
        private string rootDataPath;

        [SerializeField]
        private UnityEngine.Events.UnityEvent LoadedUIMovieEvent = new UnityEngine.Events.UnityEvent();

        [Serializable]
        public class MovieFinishEvent : UnityEngine.Events.UnityEvent<int> { }

        [SerializeField]
        private MovieFinishEvent movieFinishEvent = new MovieFinishEvent();

        //----------------------------------
        //  Init / Event
        //----------------------------------
        #region init / event
        void OnEnable() {
            LoadedUIMovieEvent.AddListener(Loaded);
            movieFinishEvent.AddListener(Finished);
        }

        void OnDisable() {
            LoadedUIMovieEvent.RemoveListener(Loaded);
            movieFinishEvent.RemoveListener(Finished);
        }

        void Loaded() {
            print("Loaded All Movie");
        }

        void Finished(int movieNum) {
            print("Finished Movie: " + movieNum);
        }

        private void Awake() {

            if (rootPath == RootPath.dataPath) {
                rootDataPath = Application.dataPath;
            } else if (rootPath == RootPath.persistentDataPath) {
                rootDataPath = Application.persistentDataPath;
            } else if (rootPath == RootPath.streamingAssetsPath) {
                rootDataPath = Application.streamingAssetsPath;
            } else {
                rootDataPath = Application.temporaryCachePath;
            }


        }

        private void Start() {
        }

        private void Update() {
            //print(mediaPlayer.Count);
        }
        #endregion

        //----------------------------------
        //  LoadUIImages
        //----------------------------------
        public void LoadUIMovies() {

            if (uiMovies.Length == 0) return;

            GameObject parentObj = new GameObject("Mediaplayers");

            for (int i = 0; i < uiMovies.Length; i++) {
                if (uiMovies[i].fileName != "") {
                    // create mediaplayer
                    GameObject obj = new GameObject("mediaplayer_" + uiMovies[i].fileName);
                    obj.transform.SetParent(parentObj.transform);
                    MediaPlayer player = obj.AddComponent<MediaPlayer>();
                    player.m_Loop = uiMovies[i].loop;


                    player.OpenVideoFromFile(
                        MediaPlayer.FileLocation.AbsolutePathOrURL,
                        rootDataPath + movieDirPath + uiMovies[i].fileName,
                        false
                    );
                    player.Events.AddListener(OnVideoEvent);

                    // create displayui
                    DisplayUGUI movie = uiMovies[i].obj.GetComponent<DisplayUGUI>();
                    if (movie == null) {
                        movie = uiMovies[i].obj.AddComponent<DisplayUGUI>();
                        movie._noDefaultDisplay = false;
                        movie._displayInEditor = false;
                        movie.color = new Color(0, 0, 0, 0);
                        movie._mediaPlayer = player;
                    } else {
                        movie._mediaPlayer = player;
                    }
                    mediaPlayer.Add(player);
                }
            }

        }

        private int readyToPlayCount = 0;
        private void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode error) {
            switch (et) {
                case MediaPlayerEvent.EventType.FinishedPlaying:
                    print("FinishedPlaying");
                    print(mp.name);
                    movieFinishEvent.Invoke(GetMovieNum(mp.name));
                    break;
                case MediaPlayerEvent.EventType.ReadyToPlay:
                    readyToPlayCount++;

                    if (readyToPlayCount == uiMovies.Length)
                        LoadedUIMovieEvent.Invoke();
                    break;
                    /*case MediaPlayerEvent.EventType.MetaDataReady:
						Debug.Log("MediaPlayerEvent:MetaDataReady");
						break;

					case MediaPlayerEvent.EventType.ReadyToPlay:
						Debug.Log("MediaPlayerEvent:ReadyToPlay");
						//mp.Control.Play();
						break;
					case MediaPlayerEvent.EventType.FirstFrameReady:
						Debug.Log("MediaPlayerEvent:FirstFrameReady");
						break;
						break;
					case MediaPlayerEvent.EventType.Started:
						Debug.Log("MediaPlayerEvent:Started");
						break;*/
            }
        }

        private int GetMovieNum(string playerName) {
            int num = -1;

            for (int i = 0; i < mediaPlayer.Count; i++) {
                if (playerName == mediaPlayer[i].name) {
                    num = i;
                    break;
                }
            }

            return num;
        }

        //----------------------------------
        //  Control
        //----------------------------------
        #region Control
        public void Play(int num) {
            Seek(num, 0);
            mediaPlayer[num].Control.SetVolume(uiMovies[num].volume);
            mediaPlayer[num].Control.Play();
        }

        public void Pause(int num) {
            mediaPlayer[num].Control.Pause();
        }

        public bool IsPlay(int num) {
            return mediaPlayer[num].Control.IsPlaying();
        }

        public bool IsPause(int num) {
            return mediaPlayer[num].Control.IsPaused();
        }

        public void Stop(int num) {
            mediaPlayer[num].Control.Stop();
        }

        public void Rewind(int num) {
            mediaPlayer[num].Control.Rewind();
        }

        public void Seek(int num, float seekTime) {
            mediaPlayer[num].Control.Seek(seekTime);
        }

        public float GetCurrentTimeMs(int num) {
            return mediaPlayer[num].Control.GetCurrentTimeMs();
        }
        public float GetCurrentTimeSec(int num) {
            return mediaPlayer[num].Control.GetCurrentTimeMs() / 1000f;
        }
        #endregion

        //----------------------------------
        //  UnloadAllMovies
        //----------------------------------
        #region unload
        public void UnloadMovie(int num) {
            mediaPlayer[num].Control.Stop();
            mediaPlayer[num].Control.CloseVideo();
            mediaPlayer[num] = null;
        }

        public void UnloadAllMovies() {

            for (int i = 0; i < uiMovies.Length; i++) {
                mediaPlayer[i].Control.Stop();
                mediaPlayer[i].Control.CloseVideo();
                mediaPlayer[i] = null;
            }
            mediaPlayer.Clear();

        }
        #endregion
    }
}
