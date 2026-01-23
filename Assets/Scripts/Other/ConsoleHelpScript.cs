using System.Linq;
using Commands;
using UnityEngine;

public class ConsoleHelpScript : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text text;
    
    private void Start()
    {
        CommandsManager.Instance.RegisterInstance(this);
    }
    
    [Command("help", "List all available commands")]
    public void Help()
    {
        var list = CommandsManager.Instance.GetAllCommands().ToList();
        if (list.Count == 0)
        {
            Debug.Log("No commands registered.");
            return;
        }
        
        text.text = string.Join("\n", list.Select(c => $"- {c.name}: {c.description}"));
    }
}
