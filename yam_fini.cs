using System;
using System.IO;
using System.Threading;
using Internal;

using System.Diagnostics;
using System.Text.RegularExpressions;

public struct GameData    ////// Structure pour les données de la partie //////
{                         /// Contient les structures Parameters
                          /// Player, Round et FinalResults
    public Parameters Parameters;
    public Player[] Players;
    public Round[] Rounds;
    public FinalResult[] FinalResults;
}
public struct Parameters    ////// Structure pour les paramètres //////
{
    public string Code;         // Le code de la partie 
    public string Date;         // Stocké comme chaîne pour simplifier
}
public struct Player    ////// Structure pour les joueurs //////
{
    public int Id;              // L'identifiant du joueur
    public string Pseudo;       //Le pseudo du joueur
}
public struct Round   ////// Structure pour les tours //////
{
    public int Id;              // L'identifiant de tour allant de 1 à 13
    public Result[] Results;    // Tableau de la structure Result
}
public struct Result    ////// Structure pour les Résultats d'un tour précis //////
{
    public int IdPlayer;        // L'identifiant du joueur 
    public int[] Dice;          // Tableau contenant les résultats de chaque dés à la fin du tour
    public string Challenge;    // Le nom du challenge choisi
    public int Score;           // Le score obtenu par le joueur pour le challenge choisi
}
public struct FinalResult   ////// Structure pour les Résultats globaux //////
{
    public int IdPlayer;       // L'identifiant du joueur
    public int Bonus;         // Le score du bonus obtenu
    public int Score;         // Le score obtenu a la fin de la partie avec le bonus
}

struct Joueur {
  public int num;     //identifiant joueur
  public string nom;    //pseudo
  public int score;     //score actuel
  public int score_min;     //score d'avancement du bonus
  public int score_total;   //score final (calculé à la fin)
  public bool[] Challenges;     //stocke les challenges déjà réalisés
}




class YAMS {
  public static string Tab(int t) {   //Renvoie t tabulations 
    string tabs = "";
    for (int i=0; i<t; i++) {
      tabs+= "\t";
    }
    return tabs;
  }

  public static string RenvoieJsonDATA(GameData DATA) {   //Utilise DATA pour générer le Json dans une string
    string s="{\n";
    s+="\t\"parameters\": {\n";
    s+="\t\t\"code\": \""+DATA.Parameters.Code+"\",\n";
    s+="\t\t\"date\": \""+DATA.Parameters.Date+"\"\n";
    s+="\t},\n";
    s+="\t\"players\": [\n";
    for (int i=0; i<2; i++) {
      s+="\t{\n";
      s+="\t\t\"id\": "+(i+1)+",\n";
      s+="\t\t\"pseudo\": \""+DATA.Players[i].Pseudo+"\"\n\t}";
      if (i==0){s+=",";}
      s+="\n";
    }
    s+="\t]\n,";
    s+="\t\"rounds\": [\n";
    for (int i=0; i<13; i++) {
      s+="\t\t{\n";
      s+="\t\t\"id\": "+(i+1)+",\n";
      s+="\t\t\"results\": [\n";
      for (int j=0; j<2; j++) {
        s+=Tab(3)+"{\n";
        s+=Tab(3)+"\"id_player\": "+(j+1)+",\n";
        int[] T = DATA.Rounds[i].Results[j].Dice;
        s+=Tab(3)+"\"dice\": ["+T[0]+","+T[1]+","+T[2]+","+T[3]+","+T[4]+"],\n";
        s+=Tab(3)+"\"challenge\": \""+DATA.Rounds[i].Results[j].Challenge+"\",\n";
        s+=Tab(3)+"\"score\": "+DATA.Rounds[i].Results[j].Score+"\n"+Tab(3)+"}";
        if (j==0){s+=",";}
        s+="\n";
      }
      s+="\t\t]\n";
      s+="\t}";
      if (i<12){s+=",";}
      s+="\n"; 
    }
    s+="\t\t],\n";
    s+="\t\"final_result\": [\n";
    for (int i=0; i<2; i++) {
      s+="\t\t{\n";
      s+="\t\t\"id_player\" :"+(i+1)+",\n";
      s+="\t\t\"bonus\" :"+DATA.FinalResults[i].Bonus+",\n";
      s+="\t\t\"score\" :"+DATA.FinalResults[i].Score+"\n";
      s+="\t\t}";
      if (i==0){s+=",";}
      s+="\n";
    }
    s+="\t]\n";
    s+="}";

    return s;
  }



  public static void Ecrire(GameData DATA){       //Ecrit la structure DATA en json dans partie.json
        StreamWriter f = new StreamWriter(DATA.Parameters.Code+".json");
        f.WriteLine(RenvoieJsonDATA(DATA));
        f.Close();
    }



  public static void EnvoieJsonDansAPI(string fileName) {       //Fonction optionnelle
      // Détermine si le système d'exploitation est Windows (true si Windows, false sinon).
      bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;    

      // Prépare une commande pour envoyer un fichier JSON à un site web via une requête POST et récupérer un identifiant.
      var processStartInfo = new ProcessStartInfo {
          // Définit le programme à exécuter : "cmd.exe" pour Windows ou "/bin/bash" pour les autres systèmes.
          FileName = isWindows ? "cmd.exe" : "/bin/bash",     

          // Spécifie les arguments de la commande en fonction de l'OS.
          Arguments = isWindows ? 
              // Commande pour Windows
              $"/C curl -s -X POST -F \"file=@{fileName}.json\" http://yams.iutrs.unistra.fr:3000 2>nul && powershell -Command \"(Get-Content response.txt | ForEach-Object {{ $_.Substring(30, $_.Length - 35) }} | Select-Object -Last 5 | Select-Object -First 1)\""
              : 
              // Commande pour les autres systèmes (Linux/Unix/MacOS)
              $"-c \"curl -s -X POST -F 'file=@{fileName}.json' http://yams.iutrs.unistra.fr:3000 2>/dev/null | tail -n 5 | head -n 1\"",

          // Utilise les arguments en mode shell pour permettre l'exécution des commandes dans le terminal.
          UseShellExecute = false,
          // Redirige la sortie standard pour pouvoir lire les données renvoyées par la commande.
          RedirectStandardOutput = true,
          // Masque la fenêtre de terminal lors de l'exécution de la commande.
          WindowStyle = ProcessWindowStyle.Hidden
      };

      // Démarre le processus en utilisant les informations configurées dans `processStartInfo`.
      var process = Process.Start(processStartInfo);      

      // Lit tout le contenu de la sortie standard (les données renvoyées par la commande).
      string output = process.StandardOutput.ReadToEnd();     

      // Attend la fin de l'exécution du processus avant de continuer.
      process.WaitForExit();

      // Utilise une expression régulière pour rechercher une ligne contenant "Identifiant de la partie : [quelque chose]".
      var match = Regex.Match(output, @"Identifiant de la partie : ([a-zA-Z0-9]+)");      

      // Si un résultat correspondant à l'expression régulière est trouvé.
      if (match.Success) {
          // Récupère uniquement l'identifiant à partir du groupe capturé dans l'expression régulière.
          string cleanResult = match.Groups[1].Value;       
          // Affiche l'identifiant de la partie dans la console.
          Console.WriteLine($"Identifiant de la partie : {cleanResult}\nVous pouvez dès à présent consulter le déroulement de votre partie sur notre site, dans l'onglet \"Fiche de Résultat\", en utilisant l'identifiant ci-dessus.");     
      } else {
          // Affiche un message d'erreur si aucun identifiant n'est trouvé dans la sortie.
          Console.WriteLine("Impossible de trouver l'identifiant.");
      }
  } 





  public static string Vert(string S) {        //Transforme S en vert pour affichage
    return "\u001b[32m" + S + "\u001b[0m";    //32=vert en code ANSI
  }
  public static string Rouge(string S) {      //Transforme S en rouge pour affichage
    return "\u001b[31m" + S + "\u001b[0m";    //31=rouge en ANSI
  }

  public static void Affiche(int[] T, int N) {      //AFFICHE tab de N entiers
    for (int i=0; i<N; i++) {
      Console.Write(Vert(T[i]+" "));
    }
    Console.WriteLine();
  }


  public static int[] Trie(int[] T) {     // TRIE tab de 5 entiers (tri par sélection)
    int min;
    for (int i=0; i<5; i++) {
      min = i;
      for (int j=i; j<5; j++) {
        if (T[j]<T[min]) {
          min=j;
        }
      }
      int tmp = T[i];
      T[i] = T[min];
      T[min] = tmp;
    }
    return T;
  }

    // T EST CONSIDÉRÉ COMME TRIÉ POUR LES TESTS SUIVANTS
  public static int NbDansTab(int[] T, int n) {     // COMPTE le nombre de n dans tab
    int compt = 0;
    for (int i=0; i<5; i++) {
      if (T[i]==n) {compt++;}
    }
    return compt;
  }

  public static int Brelan(int[] T) {     // TESTE un brelan et renvoie le score
    int score = 0;
    for (int i=1; i<7; i++) {
      if (NbDansTab(T,i)>=3) {
        score=i*3;
      }
    }
    return score;
  }
  public static int Carre(int[] T) {      // TESTE un carré et renvoie le score
    int score = 0;
    for (int i=1; i<7; i++) {
      if (NbDansTab(T,i)>=4) {
        score=i*4;
      }
    }
    return score;
  }
  public static int Full(int[] T) {     // TESTE un full et renvoie le score
    int score = 0;
    int n1=T[0];
    int n2=T[4];
    if ((NbDansTab(T,n1)==2 && NbDansTab(T,n2)==3) || (NbDansTab(T,n1)==3 && NbDansTab(T,n2)==2)) {
      score=25;
    }
    return score;
  }
  public static int PetiteSuite(int[] T) {      // TESTE une petite suite et renvoie le score
    int score = 0;
    bool suite=false;
    for (int j=1; j<4 && suite==false; j++) {
      suite = true;
      for (int i=j; i<j+4; i++) {
        if (NbDansTab(T,i)==0) {suite=false;}
      }
    }
    if (suite) {score=30;}
    return score;
  }
  public static int GrandeSuite(int[] T) {      // TESTE une grande suite et renvoie le score
    int score = 0;
    bool suite=false;
    for (int j=1; j<3 && suite==false; j++) {
      suite = true;
      for (int i=j; i<j+5; i++) {
        if (NbDansTab(T,i)==0) {suite=false;}
      }
    }
    if (suite) {score=40;}
    return score;
  }
  public static int Yams(int[] T) {      // TESTE un yams et renvoie le score
    int score = 0;
    int n=T[0];
    if (NbDansTab(T,n)==5) {score=50;}
    return score;
  }
  public static int Chance(int[] T) {     // TESTE une chance et renvoie le score
    int somme=0;
    for (int i=0; i<5; i++) {
      somme=somme+T[i];
    }
    return somme;
  }

  public static string DejaFait(int ind, Joueur J) {     //teste si le challenge ind est déjà fait par le joueur J
    if (J.Challenges[ind]==true) {
      return Rouge(" (Challenge déjà réalisé)");
    }
    return "";
  }
  public static void AfficheChallenges(Joueur J) {      //AFFICHE la liste des challenges et s'ils sont réalisés (début de tour)
    Console.WriteLine("Liste des challenges : ");
    for (int i=1; i<=6; i++) {
      Console.WriteLine("["+i+"]"+" Nombre de "+i + " (Somme des dés ayant obtenu "+i + ")"+DejaFait(i-1,J));
    }

    Console.WriteLine("[7] Brelan (Sommes des 3 dés identiques)"+DejaFait(6,J));
    Console.WriteLine("[8] Carré (Sommes des 4 dés identiques)"+DejaFait(7,J));
    Console.WriteLine("[9] Full (3 dés de même valeur + 2 dés de même valeur - 25 pts)"+DejaFait(8,J));
    Console.WriteLine("[10] Petite Suite (Une suite de 4 nombres - 30 pts)"+DejaFait(9,J));
    Console.WriteLine("[11] Grande Suite (Une suite de 5 nombres - 40 pts)"+DejaFait(10,J));
    Console.WriteLine("[12] Yam's (5 dés identique - 50 pts)"+DejaFait(11,J));
    Console.WriteLine("[13] Chance (La somme des dés obtenus)"+DejaFait(12,J));

    Console.WriteLine("\n-->  Avancement du bonus : "+J.score_min+" sur 63");
  }

  public static Joueur ChoixChallenge(Joueur J, int[] T, GameData DATA, int R) {      //AFFICHE les challenges et demande de choisir lequel réaliser (fin de tour)
    int[] Scores = new int[13] {0,0,0,0,0,0,Brelan(T),Carre(T),Full(T),PetiteSuite(T),GrandeSuite(T),Yams(T),Chance(T)};      //tab avec tous les scores possibles
    string[] challenges = new string[13] {"nombre1","nombre2","nombre3","nombre4","nombre5","nombre6","brelan","carre","full","petite","grande","yams","chance"};

    for (int i=1; i<=6; i++) {
      Scores[i-1] = NbDansTab(T,i)*i;
      Console.WriteLine("["+i+"]"+" Nombre de "+i+" : "+Scores[i-1]+DejaFait(i-1,J));
    }

    Console.WriteLine("[7] Brelan : "+Scores[6]+DejaFait(6,J));
    Console.WriteLine("[8] Carré : "+Scores[7]+DejaFait(7,J));
    Console.WriteLine("[9] Full : "+Scores[8]+DejaFait(8,J));
    Console.WriteLine("[10] Petite Suite : "+Scores[9]+DejaFait(9,J));
    Console.WriteLine("[11] Grande Suite : "+Scores[10]+DejaFait(10,J));
    Console.WriteLine("[12] Yam's : "+Scores[11]+DejaFait(11,J));
    Console.WriteLine("[13] Chance : "+Scores[12]+DejaFait(12,J));

    Console.WriteLine(Vert(J.nom+", faites votre choix de challenge (1-13)"));

    int ch = 0;
    bool valide = false;
    while (valide==false) {
      if (int.TryParse(Console.ReadLine(), out ch) && 0<ch &&ch<=13 && J.Challenges[ch-1] == false) {     //teste si l'input est valide
        valide=true;
      } else {
        Console.WriteLine(Rouge("Choix incorrect ou challenge déjà réalisé"));
      }
    }
    //Stockage dans la structure data
    DATA.Rounds[R-1].Results[J.num-1].Challenge = challenges[ch-1];
    for (int d=0; d<5; d++) {
      DATA.Rounds[R-1].Results[J.num-1].Dice[d] = T[d];
    }
    DATA.Rounds[R-1].Results[J.num-1].Score = Scores[ch-1];
    
    J.Challenges[ch-1] = true;        //passe le challenge à "réalisé"
    if (ch<7) {
      J.score_min=J.score_min+ Scores[ch-1];
    }
    J.score=J.score+ Scores[ch-1];
    
    return J;
  }



public static bool[] ChoixRelance() {       //Fonction qui renvoie quels dés l'utillisateur veut relancer (sous forme de tab de bool)
    bool[] Change = new bool[5] {false,false,false,false,false};
    Console.WriteLine("Entrez un par un les numéros des dés que vous souhaitez relancer (de 1 à 5), en appuyant sur \"Entrer\" après chaque numéro. Une fois vos choix terminés, appuyez sur \"Entrer\" sans rien taper pour les valider. Si vous voulez annuler vos choix, tapez \"A\". Si vous souhaitez conserver tous vos dés tels quels, appuyez directement sur \"Entrer\" sans saisir de numéro.");
    int c;
    bool fin = false;
    string input;
    while (!fin) {
      input = Console.ReadLine();
      if (input.ToLower() == "a") {          
        for (int i=0;i<5;i++) {Change[i]=false;}        //Réinitialise le tab pour annuler ses choix de relance
        Console.WriteLine(Rouge("Tous les dés ont été désélectionnés"));
      }
      else if (int.TryParse(input, out c) && 0<c && c<6) {        //Si le nombre est valide on le prend en compte
        Change[c-1] = true;
      } else {
        fin = true;                   //Sinon on considère que l'utilisateur a fait son choix
      }
    }
    return Change;
  }

  public static int[] RelanceDes(int[] T, bool[] Change) {     //Lance certains dés en fonction de sa correspondance dans Change (si true)
    Random random = new Random();
    for (int i=0; i<5; i++) {
      if (Change[i]) {
        T[i] = random.Next(1,7);
      }
    }
    return Trie(T);
  }




  public static Joueur[] InitJoueurs() {     //Initialise un tab qui stocke les deux joueurs
    Joueur[] TabJ = new Joueur[2];
    for (int i=1; i<=2; i++) {
      Console.Write("Joueur {0}, entrez votre nom : ",i);

      Joueur J = new Joueur();
      J.num = i;
      J.nom = Console.ReadLine();
      J.score = 0;
      J.score_min = 0;
      J.score_total = 0;
      J.Challenges = new bool[13] {false,false,false,false,false,false,false,false,false,false,false,false,false};      //Ses challenges sont par défaut non réalisés
      TabJ[i-1] = J;
    }
    Console.WriteLine("\n");
    return TabJ;
  }


  public static Joueur Tour(Joueur J, GameData DATA, int R) {     //Fonction qui execute un tour pour un joueur J et round R
    Console.WriteLine(Vert(J.nom+", c'est votre tour !"));
    bool[] Change = new bool[5] {true,true,true,true,true};
    int[] T = new int[5];           //tab qui stocke les 5 dés de lancer

    AfficheChallenges(J);
    for (int i=1;i<4;i++) {         //Boucle pour faire relancer les dés 3 fois
      if (i>1) { 
        Change = ChoixRelance();
        bool valider = true;
        for (int b=0; b<5; b++) {
          if (Change[b] == true) {valider=false;}
        }
      if (valider == true) {break;}       //Si aucune relance de dés, cela ne sert à rien de continuer 
      }
      Console.WriteLine(Rouge("Lancer n°"+i+" :"));
      T = RelanceDes(T,Change);
      Affiche(T,5);
    }

    J = ChoixChallenge(J,T,DATA,R);
    Console.Clear();
    Console.WriteLine("Score total de "+J.nom+" : "+J.score);
    Console.WriteLine("\n\n");
    return J;
  }
  
  public static Joueur[] ResultatFin(Joueur[] TabJ, GameData DATA) {        //Affiche les résultats rapides de fin de partie
      Console.WriteLine(Rouge(" --- PARTIE TERMINEE --- "));
      for (int j = 0; j < 2; j++) {
        TabJ[j].score_total = TabJ[j].score;
        if (TabJ[j].score_min >= 63) {
            TabJ[j].score_total += 35;
            Console.WriteLine("Joueur " + TabJ[j].num + " " + TabJ[j].nom + " : " + TabJ[j].score + Vert(" + 35") + " = " + TabJ[j].score_total);
            DATA.FinalResults[j].Bonus = 35;
        } else {
            Console.WriteLine("Joueur " + TabJ[j].num + " " + TabJ[j].nom + " : " + TabJ[j].score_total);
        }

        DATA.FinalResults[j].Score = TabJ[j].score_total;
      }
      Console.WriteLine();

      if (TabJ[0].score_total > TabJ[1].score_total) {
          Console.WriteLine(Vert("Bravo " + TabJ[0].nom + " !"));
      } else if (TabJ[0].score_total < TabJ[1].score_total) {
          Console.WriteLine(Vert("Bravo " + TabJ[1].nom + " !"));
      } else {
          Console.WriteLine(Vert("Match nul !"));
      }

      return TabJ;
  }





  public static GameData InitGameData(Joueur[] TabJ) {      //Initialise la structure GAMEDATA
    GameData DATA = new GameData();
    DATA.Parameters = new Parameters();
    DATA.Parameters.Code = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).ToString();
    DATA.Parameters.Date = DateTime.Now.ToString("yyyy-MM-dd");
    DATA.Players = new Player[2];
    for (int p=0; p<2; p++) {
      DATA.Players[p] = new Player();
      DATA.Players[p].Id = p;
      DATA.Players[p].Pseudo = TabJ[p].nom;
    }
    DATA.Rounds = new Round[13];
    for (int r=0; r<13; r++) {
      DATA.Rounds[r] = new Round();
      DATA.Rounds[r].Id = r+1;
      DATA.Rounds[r].Results = new Result[2];
      for (int p=0; p<2; p++) {
        DATA.Rounds[r].Results[p] = new Result();
        DATA.Rounds[r].Results[p].IdPlayer = p;
        DATA.Rounds[r].Results[p].Dice = new int[5];
        DATA.Rounds[r].Results[p].Challenge = "";
        DATA.Rounds[r].Results[p].Score = 0;
      }
    }
    DATA.FinalResults = new FinalResult[2];
    for (int p=0; p<2; p++) {
      DATA.FinalResults[p].IdPlayer=p;
      DATA.FinalResults[p].Bonus = 0;
      DATA.FinalResults[p].Score = 0;
    }
    
    return DATA;
  }





  public static void Main() {
    Joueur[] TabJ = InitJoueurs();
    Console.Clear();

    GameData DATA = InitGameData(TabJ);


    
    for (int R=1; R<=13; R++) {       //Boucle pour faire 13 rounds
      Console.WriteLine(Rouge("\t ROUND "+R));
      for (int j=0; j<2; j++) {         //Boucle pour faire jouer les 2 joueurs
        TabJ[j] = Tour(TabJ[j], DATA, R);
      }
    }
    Console.Clear();

    TabJ = ResultatFin(TabJ, DATA);
    Ecrire(DATA);                 //Ecrit le contenu de la structure DATA en fichier json


    //Partie pour demander à l'utilisateur la méthode d'envoie du json
    Console.WriteLine("Choisissez la méthode d'envoi des informations de la partie pour accéder aux statistiques détaillées de la partie : ");
    Console.WriteLine("[1] Métode automatique (Fichier json envoyé automatiquement sur le serveur)");
    Console.WriteLine("[2] Méthode manuelle (Fichier json à déposer sur http://yams.iutrs.unistra.fr:3000)");
    int rep = 0;
    bool rep_valide = false;
    while(!rep_valide) {
      if (int.TryParse(Console.ReadLine(),out rep) && (rep==1 || rep==2)) {
        rep_valide=true;
      } else {
        Console.WriteLine(Rouge("Choix invalide"));
      }
    }
    if (rep==1) {
      EnvoieJsonDansAPI(DATA.Parameters.Code);
    } else {
      Console.WriteLine($"Fichier généré dans le même dossier du jeu : {Directory.GetCurrentDirectory()}/{DATA.Parameters.Code}.json");
    }
  }
}