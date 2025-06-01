
# EasySave

EasySave est un logiciel de sauvegarde de fichiers developpe en C# avec le framework .NET Core. Il fait partie de la suite ProSoft et a ete concu pour vous permettre de proteger facilement vos fichiers importants, meme si vous n’etes pas informaticien.

## Contexte

Notre equipe a integre l’editeur de logiciels ProSoft. Sous la responsabilite de la Direction des Systemes d’Information, nous gerons le projet EasySave.  
Nos missions :  
- Developper et ameliorer EasySave (versions majeures et mineures)  
- Ecrire la documentation pour les utilisateurs
- Assurer la qualite et la reprise possible du projet par d’autres equipes



## Outils utilises

- Visual Studio 2019   
- draw io (pour les diagrammes)  


## Versions du logiciel

### Version 1.0

- Application console en .NET Core  
- Jusqu’a 5 travaux de sauvegarde  
- Chaque sauvegarde comprend : nom, dossier source, dossier cible, type (complete ou differentielle)  
- Execution possible d’un seul travail ou de tous les travaux a la suite  
- Compatible avec les disques locaux, externes et reseau  
- Journal quotidien (JSON ou XML, avec retours a la ligne pour lecture facile)  
- Fichier d’etat qui enregistre la progression en temps reel

Informations du journal :  
- Horodatage  
- Nom du travail de sauvegarde  
- Chemin complet du fichier source (format UNC)  
- Chemin complet du fichier de destination (UNC)  
- Taille du fichier  
- Temps de transfert en millisecondes (negatif si erreur)

Informations du fichier d’etat :  
- Horodatage  
- Nom du travail de sauvegarde  
- Statut (Actif, Inactif, etc.)  
- Nombre total de fichiers a copier  
- Taille des fichiers a copier  
- Progression  
- Fichiers restants (nombre et taille)  
- Chemin du fichier en cours (source et destination)

Les fichiers de journal et d’etat doivent etre stockes dans des emplacements adaptes aux serveurs clients (eviter par exemple C:\temp).

### Version 2.0

- Interface graphique (WPF en .NET Core), plus de console  
- Travaux illimites  
- Cryptage des fichiers via le logiciel CryptoSoft  
- Seuls les fichiers avec des extensions definies par l’utilisateur sont cryptes  
- Journal ameliore : ajout du temps de cryptage (en millisecondes)  
- Detection des logiciels metiers : si un logiciel metier est detecte, EasySave bloque la sauvegarde. En sequentiel, il termine la sauvegarde en cours puis stoppe la suite. L’utilisateur peut definir la liste des logiciels metiers dans les parametres.

### Version 3.0

- Sauvegardes en parallele : plusieurs sauvegardes peuvent s’executer en meme temps  
- Gestion des fichiers prioritaires : certains fichiers sont sauvegardes en priorite selon les reglages  
- Interdiction de transfert parallele de fichiers volumineux (parametre n Ko) pour ne pas saturer le reseau  
- Pause automatique si un logiciel metier est detecte (les sauvegardes reprennent automatiquement apres arret du logiciel metier)  
- Possibilite de mettre en pause, de relancer ou d’arreter chaque sauvegarde individuellement ou toutes ensemble  
- Affichage en temps reel de l’avancement (avec un pourcentage de progression)  
- Console de visualisation distante : permet de suivre et gerer les sauvegardes depuis un autre ordinateur via des sockets  
- CryptoSoft en mono-instance : il ne peut etre lance qu’une seule fois en meme temps  
- Option de reduction des taches paralleles si la charge reseau est trop elevee

## Installation

Prerequis :  
- Windows avec .NET Core (version 9.0 ou plus)  
- Visual Studio
- Windows OS

Telechargement avec Git :  
git clone https://github.com/faresbnm/EasySave.git
Sinon, telechargez le projet en fichier Zip.

Lancement :  
- Ouvrez le projet dans Visual Studio.  
- Compilez-le.  
- Lancez l’application.

## Fonctionnalites principales

- Interface simple et conviviale  
- Sauvegardes illimitees (v2.0 et suivantes)  
- Sauvegardes completes ou differentielles  
- Suivi en temps reel de la progression  
- Cryptage automatique des fichiers sensibles (v2.0 et suivantes)  
- Choix du format de journal (JSON ou XML)  
- Blocage automatique si un logiciel metier est actif  
- Multilingue (francais et anglais)

## Comment l’utiliser

1. Choisir la langue (francais ou anglais).  
2. Choisir le format de journalisation (JSON ou XML).  
3. Dans le menu, vous pouvez :  
- Lister les sauvegardes existantes  
- Creer une nouvelle sauvegarde  
- Modifier une sauvegarde  
- Supprimer une sauvegarde  
- Lancer une sauvegarde  
- Quitter l’application  



## Securite et confidentialite

- Cryptage des fichiers confidentiels avec CryptoSoft (v2.0 et suivantes).  
- Blocage automatique des sauvegardes si un logiciel metier est ouvert.  
- Ecriture d’un journal complet de toutes les actions.

-------------------------------------------------------------in english -------------------------------------------------------------------------

# EasySave

EasySave is a file backup software developed in C# with the .NET Core framework. It is part of the ProSoft suite and was designed to help you easily protect your important files, even if you are not an IT professional.

## Context

Our team joined the software publisher ProSoft. Under the responsibility of the Information Systems Department, we manage the EasySave project.  
Our missions:  
- Develop and improve EasySave (major and minor versions)  
- Write documentation for users and customer support  
- Ensure the quality of the project and that it can be handed over to other teams



## Tools used

- Visual Studio 2019 
- draw io (for diagrams)  


## Software versions

### Version 1.0

- Console application in .NET Core  
- Up to 5 backup jobs  
- Each backup includes: name, source folder, target folder, type (full or differential)  
- Option to run one backup job or all jobs sequentially  
- Compatible with local, external and network drives  
- Daily log file (JSON or XML, with line breaks for easy reading)  
- Status file that tracks progress in real-time

Log file information:  
- Timestamp  
- Backup job name  
- Full source file path (UNC format)  
- Full destination file path (UNC format)  
- File size  
- Transfer time in milliseconds (negative if error)

Status file information:  
- Timestamp  
- Backup job name  
- Status (Active, Inactive, etc.)  
- Total number of files to copy  
- Size of files to copy  
- Progress  
- Remaining files (number and size)  
- Current file path (source and destination)

The log and status files must be stored in appropriate locations on client servers (avoid using locations like C:\temp).

### Version 2.0

- Graphical interface (WPF in .NET Core), no more console  
- Unlimited backup jobs  
- File encryption via the CryptoSoft software  
- Only files with user-defined extensions are encrypted  
- Enhanced log file: encryption time added (in milliseconds)  
- Business software detection: if a business application is detected, EasySave stops the backup. In sequential mode, it finishes the current backup job and stops before launching the next one. The user can define the list of business software in the settings.

### Version 3.0

- Parallel backups: multiple backups can run at the same time  
- Management of priority files: some files are saved first according to the settings  
- No simultaneous transfer of large files (n KB parameter) to avoid saturating the network  
- Automatic pause if business software is detected (backups automatically resume when the software is closed)  
- Ability to pause, resume, or stop each backup job individually or all together  
- Real-time progress display (with a percentage)  
- Remote console: allows tracking and managing backups from another computer via sockets  
- CryptoSoft as a single instance: it can only run once at a time  
- Option to reduce parallel tasks if the network load is too high

## Installation

Requirements:  
- Windows with .NET Core (version 4.7.2 or later)  
- Visual Studio  
- Windows OS

Download with Git:  
git clone https://github.com/faresbnm/EasySave.git  
Or download the project as a Zip file.

Running the application:  
- Open the project in Visual Studio  
- Build it  
- Run the application

## Main features

- Simple and user-friendly interface  
- Unlimited backups (v2.0 and later)  
- Full or differential backups  
- Real-time progress tracking  
- Automatic encryption of sensitive files (v2.0 and later)  
- Choice of log file format (JSON or XML)  
- Automatic blocking if business software is active  
- Multilingual (French and English)

## How to use it

1. Choose the language (French or English)  
2. Choose the log format (JSON or XML)  
3. In the menu, you can:  
- List existing backups  
- Create a new backup  
- Update a backup  
- Delete a backup  
- Run a backup  
- Exit the application  



## Security and confidentiality

- Encryption of sensitive files with CryptoSoft (v2.0 and later)  
- Automatic blocking of backups if business software is detected  
- Complete log file of all actions

## Support and documentation

All documentation is available in the project.  
For any questions, contact the support team.

EasySave: simple, secure, and ready for your backup needs.
