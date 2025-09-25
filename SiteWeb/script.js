    let donneesCompletJeu; // Cette variable va contenir toutes les données du jeu, telles que les paramètres, les joueurs, les résultats des tours, et les résultats finaux.
    let indiceDuTourEnCours = 0; // Cette variable garde la trace du tour actuel en cours, commence à 0 pour le premier tour.
    let scoresTotalDesJoueurs = [0, 0]; // Les scores des deux joueurs, initialement à zéro.
    let bonusJoueurs = [0, 0]; // Les bonus des joueurs, initialement à zéro.
    const sequenceCode = ['ArrowUp', 'ArrowUp', 'ArrowDown', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'ArrowLeft', 'ArrowRight', 'b', 'a', 'Enter']; // Une séquence de touches définie (comme un code secret ou cheat code).
    let entreesUtilisateurActuelles = []; // Une liste qui conserve les touches que le joueur a appuyées pour vérifier la séquence entrée.
    let nomFichier;
    document.addEventListener('wheel', gererEvenementDeDefilement, { passive: false }); // Écoute l'événement de défilement de la page, pour appeler la fonction gererEvenementDeDefilement.
    document.addEventListener("keydown", gererSaisieUtilisateur); // Écoute l'événement de pression d'une touche, pour appeler la fonction gererSaisieUtilisateur.

    function gererEvenementDeDefilement(event) {
        // Vérifie si l'utilisateur est en haut de la page et tente de défiler vers le haut.
        // window.scrollY === 0 : Cela signifie que la page est déjà tout en haut.
        // event.deltaY < 0 : L'utilisateur essaie de défiler vers le haut.

        // Vérifie si l'utilisateur est en bas de la page et tente de défiler vers le bas.
        // window.innerHeight + window.scrollY : La position actuelle en bas de la fenêtre visible.
        // document.body.offsetHeight : La hauteur totale du contenu de la page.
        // Ces deux valeurs égales ou supérieures indiquent qu'on est au bas de la page.
        if ((window.scrollY === 0 && event.deltaY < 0) || (window.innerHeight + window.scrollY >= document.body.offsetHeight && event.deltaY > 0)) {            
            // Empêche le comportement de défilement par défaut dans ces cas précis.
            event.preventDefault();
        }
    }
    
    function gererSaisieUtilisateur(event) {    
        // Vérifie si l'utilisateur a appuyé sur la touche "Enter".
        if (event.key === "Enter") {
            // Empêche le comportement par défaut de la touche "Enter" (par exemple, soumettre un formulaire).
            event.preventDefault();
            // Appelle une fonction pour charger les données du jeu.
            chargerDonneesJeu();
        } 
        // Vérifie si l'utilisateur a appuyé sur la flèche droite ("ArrowRight").
        else if (event.key === "ArrowRight") {
            // Empêche le comportement par défaut de la flèche droite (comme le défilement).
            event.preventDefault();
            // Appelle une fonction pour afficher le tour suivant du jeu.
            afficherTourSuivant();
        } 
        // Vérifie si l'utilisateur a appuyé sur la flèche gauche ("ArrowLeft").
        else if (event.key === "ArrowLeft") {
            // Empêche le comportement par défaut de la flèche gauche (comme le défilement).
            event.preventDefault();
            // Appelle une fonction pour afficher le tour précédent du jeu.
            afficherTourPrecedent();
        }
    }
    

    function chargerDonneesJeu() {
        // Ajoute la classe 'loading' à tous les éléments de la page pour signaler que le chargement est en cours.
        document.body.querySelectorAll('*').forEach(element => element.classList.add('loading'));
        // Récupère le nom du fichier à charger depuis un champ d'entrée HTML avec l'id "nomFichier".
        // Si aucun nom n'est saisi, utilise "tx68ar7tor" comme nom de fichier par défaut.
        const nomFichier = document.getElementById("nomFichier").value || "tx68ar7tor";
        console.log(nomFichier)
        
        // Réinitialise les données du jeu pour partir d'un état vide.
        reinitialiserJeu();
        
        // Initialise un objet vide pour stocker les données du jeu.
        donneesCompletJeu = {
            parameters: {},   // Stocke les paramètres généraux du jeu.
            players: [],      // Stocke les informations des joueurs.
            rounds: [],       // Stocke les données des tours (rounds).
            final_result: []  // Stocke les résultats finaux.
        };
        
        /**
         * Fonction utilitaire pour récupérer des données JSON depuis un endpoint spécifique.
         * @param {string} endpoint - Le nom de la ressource à récupérer.
         * @returns {Promise} - Une promesse pour récupérer les données JSON.
         */
        function recupererDonneesDepuisEndpoint(endpoint) {
            // Construit l'URL d'accès à l'API en utilisant le nom du fichier et l'endpoint fourni.
            return fetch(`http://yams.iutrs.unistra.fr:3000/api/games/${nomFichier}/${endpoint}`)
                .then(response => {
                    // Vérifie si la réponse est valide (code HTTP entre 200 et 299).
                    if (!response.ok) {
                        // Lève une erreur avec le code HTTP si la réponse est invalide.
                        throw new Error(`Erreur HTTP ${response.status} pour ${endpoint}`);
                    }
                    // Convertit la réponse en JSON et la renvoie.
                    return response.json();
                });
        }
        
        // Étape 1 : Récupère les paramètres du jeu.
        recupererDonneesDepuisEndpoint("parameters")
            .then(data => {
                // Stocke les paramètres récupérés dans l'objet donneesCompletJeu.
                donneesCompletJeu.parameters = data;
        
                // Étape 2 : Récupère les informations des joueurs.
                return recupererDonneesDepuisEndpoint("players");
            })
            .then(data => {
                // Stocke les informations des joueurs dans donneesCompletJeu.
                donneesCompletJeu.players = data;
        
                // Étape 3 : Prépare la récupération des données pour les 13 tours (rounds).
                let roundPromises = [];
        
                // Utilise une boucle for pour ajouter une promesse pour chaque tour.
                for (let i = 0; i < 13; i++) {
                    roundPromises.push(recupererDonneesDepuisEndpoint(`rounds/${i + 1}`)); // Appelle recupererDonneesDepuisEndpoint pour chaque tour (de 1 à 13).
                }
        
                // Attend que toutes les promesses soient résolues (tous les rounds chargés).
                return Promise.all(roundPromises);
            })
            .then(rounds => {
                // Stocke les données de chaque tour dans donneesCompletJeu.
                donneesCompletJeu.rounds = rounds;
        
                // Étape 4 : Récupère les résultats finaux du jeu.
                return recupererDonneesDepuisEndpoint("final-result");
            })
            .then(data => {
                // Stocke les résultats finaux dans donneesCompletJeu.
                donneesCompletJeu.final_result = data;
        
                // Affiche les boutons de navigation en supprimant la classe 'invisible'.
                document.getElementById('btnvueglobale').classList.remove("invisible");
                document.getElementById('btnvuetour').classList.remove("invisible");
            })
            .catch(error => {
                // En cas d'erreur pendant le chargement, affiche un message dans la console.
                console.error("Erreur de chargement des données JSON :", error);
        
                // Affiche une alerte à l'utilisateur pour l'informer de l'erreur.
                alert("Erreur de chargement des données JSON. Vérifiez le nom du fichier et réessayez.");
            })
            .finally(() => {
                // Que le chargement soit réussi ou échoué, enlève la classe 'loading' sur tous les éléments.
                document.body.querySelectorAll('*').forEach(element => element.classList.remove('loading'));
            });
    }
    
    function afficherVueDensembleDuJeu() {
            // Affiche la vue globale du jeu (résumé) et cache la vue par tour.
            document.getElementById("vueGlobale").style.display = "flex";
            document.getElementById("vueTour").style.display = "none";
        
            const globalSummary = document.getElementById("resumeGlobal");
            const gameParameters = donneesCompletJeu.parameters;
        
            let tableauHTML = `
                <h3>Résumé de la partie</h3>
                <p><strong>Code de Jeu:</strong> ${gameParameters.code}</p>
                <p><strong>Date:</strong> ${gameParameters.date}</p>
                <table>
                    <thead>
                        <tr>
                            <th>Tour</th>
                            <th>Joueur 1 (${donneesCompletJeu.players[0].pseudo})</th>
                            <th>Score</th>
                            <th>Joueur 2 (${donneesCompletJeu.players[1].pseudo})</th>
                            <th>Score</th>
                        </tr>
                    </thead>
                    <tbody>
            `;
        
            // Pour chaque tour du jeu, ajoute une ligne au tableau avec les résultats des joueurs.
            donneesCompletJeu.rounds.forEach((round, index) => {
                const joueur1Result = round.results.find(result => result.id_player === donneesCompletJeu.players[0].id);
                const joueur2Result = round.results.find(result => result.id_player === donneesCompletJeu.players[1].id);
        
                tableauHTML += `
                    <tr>
                        <td>Tour ${index + 1}</td>
                        <td>${joueur1Result.challenge} : ${joueur1Result.dice.join(", ")}</td>
                        <td>${joueur1Result.score}</td>
                        <td>${joueur2Result.challenge} : ${joueur2Result.dice.join(", ")}</td>
                        <td>${joueur2Result.score}</td>
                    </tr>
                `;
            });
        
            // Ajoute les résultats finaux des joueurs à la fin du tableau.
            const joueur1Final = donneesCompletJeu.final_result.find(res => res.id_player === donneesCompletJeu.players[0].id);
            const joueur2Final = donneesCompletJeu.final_result.find(res => res.id_player === donneesCompletJeu.players[1].id);
        
            tableauHTML += `
                    </tbody>
                    <tfoot>
                        <tr>
                            <td>Résultats finaux</td>
                            <td>Bonus : ${joueur1Final.bonus}</td>
                            <td>Score total : ${joueur1Final.score}</td>
                            <td>Bonus : ${joueur2Final.bonus}</td>
                            <td>Score total : ${joueur2Final.score}</td>
                        </tr>
                    </tfoot>
                </table>
            `;
        
            // Met à jour l'affichage du résumé global avec le tableau créé.
            globalSummary.innerHTML = tableauHTML;
    };
    
    function afficherVuePourChaqueTourDeJeu() {
        // Réinitialise l'index du tour à 0 pour commencer au premier tour.
        indiceDuTourEnCours = 0;
    
        // Cache la vue globale pour se concentrer uniquement sur la vue par tour.
        document.getElementById("vueGlobale").style.display = "none";
    
        // Affiche la vue par tour.
        document.getElementById("vueTour").style.display = "flex";
    
        // Appelle la fonction pour afficher le tour actuel du jeu.
        afficherTourActuel();
    }
    
    function actualiserImageResultats(joueur1Result, joueur2Result) {
        const turnDisplay = document.getElementById("tourActuel");
    
        // Affiche les images des dés pour chaque joueur, chaque dé est représenté par une image.
        // `map` est utilisé pour créer une liste d'images HTML à partir des résultats des dés de chaque joueur.
        const diceImages1 = joueur1Result.dice.map(dice => `<img src="./Images/Dés_clair/${dice}.png" height="30rem" width="30rem" alt="${dice}">`).join('');
        const diceImages2 = joueur2Result.dice.map(dice => `<img src="./Images/Dés_clair/${dice}.png" height="30rem" width="30rem" alt="${dice}">`).join('');
    
        // Met à jour l'affichage du tour actuel avec les images des dés et les scores des joueurs.
        turnDisplay.innerHTML = `
            <h3>Tour ${indiceDuTourEnCours + 1}</h3>
            <div class="players">
                <div class="player">
                    <h4>${donneesCompletJeu.players[0].pseudo} :</h4>
                    <div class="des">${diceImages1}</div>
                    <p> - Challenge : ${joueur1Result.challenge}</p>
                    <p> - Points : ${joueur1Result.score}</p>
                </div>
                <div class="player">
                    <h4>${donneesCompletJeu.players[1].pseudo} :</h4>
                    <div class="des">${diceImages2}</div>
                    <p> - Challenge : ${joueur2Result.challenge}</p>
                    <p> - Points : ${joueur2Result.score}</p>
                </div>
            </div>
        `;
    }
    

    function afficherTourPrecedent() {
        // Si on n'est pas déjà au premier tour, on revient au tour précédent.
        if (indiceDuTourEnCours > 0) {
            // Diminue l'index du tour pour afficher le tour précédent.
            indiceDuTourEnCours--;
    
            // Affiche le tour précédent.
            afficherTourActuel();
        }
    }
    

    function afficherTourActuel() {
    
        // Sélectionne tous les boutons de navigation présents sur la page.
        const navigationButtons = document.querySelectorAll('.navigation button');
        
        // Si on est déjà au premier tour, désactive le bouton de retour en arrière.
        navigationButtons[0].classList.toggle('disabled', indiceDuTourEnCours === 0);
        
        // Si on est déjà au dernier tour, désactive le bouton de navigation au tour suivant.
        navigationButtons[1].classList.toggle('disabled', indiceDuTourEnCours === donneesCompletJeu.rounds.length - 1);
        
        // Les joueurs du jeu.
        const joueur1 = donneesCompletJeu.players[0];
        const joueur2 = donneesCompletJeu.players[1];
        
        // Réinitialisation des scores à chaque tour pour éviter des accumulations erronées.
        scoresTotalDesJoueurs = [0, 0];
        
        // Calcule les scores cumulés des joueurs jusqu'au tour actuel.
        for (let i = 0; i <= indiceDuTourEnCours; i++) {
            const turn = donneesCompletJeu.rounds[i];
            const joueur1Result = turn.results.find(result => result.id_player === joueur1.id);
            const joueur2Result = turn.results.find(result => result.id_player === joueur2.id);
        
            scoresTotalDesJoueurs[0] += joueur1Result.score;
            scoresTotalDesJoueurs[1] += joueur2Result.score;
        }
        
        // Récupération des résultats du tour actuel.
        const currentTurn = donneesCompletJeu.rounds[indiceDuTourEnCours];
        const joueur1Result = currentTurn.results.find(result => result.id_player === joueur1.id);
        const joueur2Result = currentTurn.results.find(result => result.id_player === joueur2.id);
        
        // Actualisation des images des dés pour chaque joueur.
        actualiserImageResultats(joueur1Result, joueur2Result);
        
        // Affichage des scores actuels dans l'interface utilisateur.
        document.getElementById("scoreJoueur1").innerHTML = `${joueur1.pseudo} : ${scoresTotalDesJoueurs[0]}`;
        document.getElementById("scoreJoueur2").innerHTML = `${joueur2.pseudo} : ${scoresTotalDesJoueurs[1]}`;
        
        // Si on est au dernier tour, il est nécessaire de considérer les bonus pour calculer les scores finaux.
        if (indiceDuTourEnCours === donneesCompletJeu.rounds.length - 1) {
            const joueur1FinalResult = donneesCompletJeu.final_result.find(res => res.id_player === joueur1.id);
            const joueur2FinalResult = donneesCompletJeu.final_result.find(res => res.id_player === joueur2.id);
        
            const finalScore1 = scoresTotalDesJoueurs[0] + joueur1FinalResult.bonus;
            const finalScore2 = scoresTotalDesJoueurs[1] + joueur2FinalResult.bonus;
        
            document.getElementById("scoreJoueur1").innerHTML = `${joueur1.pseudo} : ${finalScore1} pts (dont ${joueur1FinalResult.bonus} pts bonus)`;
            document.getElementById("scoreJoueur2").innerHTML = `${joueur2.pseudo} : ${finalScore2} pts (dont ${joueur2FinalResult.bonus} pts bonus)`;
        }
    }
    
    
    function afficherTourSuivant() {
        // Vérifie si on n'est pas déjà au dernier tour.
        if (indiceDuTourEnCours < 12) {
            // Augmente l'index du tour pour passer au tour suivant.
            indiceDuTourEnCours++;
    
            // Appelle la fonction pour afficher le tour actuel.
            afficherTourActuel();
        }
    }

    function reinitialiserJeu() {
        // Réinitialise toutes les données du jeu.
        donneesCompletJeu = null; // Efface les données du jeu actuelles.
        indiceDuTourEnCours = 0; // Réinitialise l'index du tour à 0.
        scoresTotalDesJoueurs = [0, 0]; // Réinitialise les scores totaux des joueurs à zéro.
        bonusJoueurs = [0, 0]; // Réinitialise les bonus des joueurs à zéro.
        document.getElementById("resumeGlobal").innerHTML = ""; // Efface le résumé global de la page.
        document.getElementById("tourActuel").innerHTML = ""; // Efface les informations du tour actuel.
        document.getElementById("scoreJoueur1").innerHTML = ""; // Efface le score du joueur 1.
        document.getElementById("scoreJoueur2").innerHTML = ""; // Efface le score du joueur 2.
        document.getElementById("vueGlobale").style.display = "none"; // Cache la vue globale.
        document.getElementById("vueTour").style.display = "none"; // Cache la vue par tour.
        document.getElementById('btnvueglobale').classList.add("invisible"); // Masque le bouton pour afficher la vue globale.
        document.getElementById('btnvuetour').classList.add("invisible"); // Masque le bouton pour afficher la vue par tour.
        document.getElementById("nomFichier").value = ""; // Vide le champ du nom du fichier.
    }
    