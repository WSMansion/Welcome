using System;
using System.Collections.Generic;
using WelCome.Core;
using WelCome.Data;
using WelCome.Models;

namespace WelCome.Systems
{
    public class AVGSystem : Singleton<AVGSystem>
    {
        private readonly List<AVGDialogueLine> _currentLines = new();
        private int _currentIndex;
        private int _currentStoryId;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;
        public AVGDialogueLine CurrentLine =>
            _isPlaying && _currentIndex < _currentLines.Count ? _currentLines[_currentIndex] : null;

        public event Action<AVGDialogueLine> OnLineDisplayed;
        public event Action OnDialogueEnded;

        public void StartDialogue(int storyId)
        {
            if (!ConfigManager.Instance.IsLoaded) return;

            var story = ConfigManager.Instance.Tables.TbAvgStory.GetOrDefault(storyId);
            if (story == null) return;

            _currentStoryId = storyId;
            _currentLines.Clear();
            _currentIndex = 0;

            for (int id = story.IdStart; id <= story.IdEnd; id++)
            {
                var evt = ConfigManager.Instance.Tables.TbAvgEvent.GetOrDefault(id);
                if (evt == null) continue;

                _currentLines.Add(new AVGDialogueLine
                {
                    Id = evt.Id,
                    Speaker = evt.Speaker,
                    Text = evt.Text,
                    Delay = evt.TimeDelay,
                    Duration = evt.TimeDuration
                });
            }

            _isPlaying = true;
            EventBus.Publish(new AVGDialogueStartedEvent { StoryId = storyId });
            DisplayCurrentLine();
        }

        public void Advance()
        {
            if (!_isPlaying) return;

            _currentIndex++;
            if (_currentIndex >= _currentLines.Count)
            {
                EndDialogue();
                return;
            }
            DisplayCurrentLine();
        }

        public void Skip()
        {
            EndDialogue();
        }

        private void DisplayCurrentLine()
        {
            var line = CurrentLine;
            if (line != null)
                OnLineDisplayed?.Invoke(line);
        }

        private void EndDialogue()
        {
            _isPlaying = false;
            _currentLines.Clear();
            _currentIndex = 0;
            OnDialogueEnded?.Invoke();
            EventBus.Publish(new AVGDialogueEndedEvent { StoryId = _currentStoryId });
        }
    }
}
