namespace Parser.Settings;

public class TokenInfo
{

    public bool Enabled { 
        
        get; 
        set;
        
    }


    public required string Type
    {
        get;
        set;
    }


    public required string Start
    {
        get;
        set; 
    }

    public string End
    {
        get;
        set;
    }
    
    
   
}