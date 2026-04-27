using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI potText;
    public TextMeshProUGUI playerChipsText;
    public TextMeshProUGUI aiChipsText;
    public TextMeshProUGUI gameMessageText;
    bool waitingForPlayer = false;
    public int playerChips = 1000;
    public int aiChips = 1000;

    public int pot = 0;
    public int currentBet = 50;

    public int playerBet = 0;
    public int aiBet = 0;

    public bool playerFolded = false;
    public bool aiFolded = false;

    public DeckManager deck;
    public GameObject cardPrefab;

    public GameObject raisePanel;
    public TMP_InputField raiseInputField;

    public Transform handSlot1;
    public Transform handSlot2;
    public Transform aiHandSlot1;
    public Transform aiHandSlot2;
    public Sprite cardBackSprite;
    private GameObject aiCardObj1;
    private GameObject aiCardObj2;

    public Transform flop1Slot;
    public Transform flop2Slot;
    public Transform flop3Slot;
    public Transform turnSlot;
    public Transform riverSlot;

    private List<CardData> playerHand = new List<CardData>();
    private List<CardData> aiHand = new List<CardData>();
    private List<CardData> communityCards = new List<CardData>();

    private List<GameObject> cardsOnTable = new List<GameObject>();
    

    private int roundStep = 0;
    private string playerLastAction = "";
    private int playerLastRaiseAmount = 0;
    private bool waitingForPlayerResponseToRaise = false;



    private void Start()
    {
        StartRound();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !waitingForPlayer)
        {
            NextStep();
        }
    }


    private void StartRound()
    {
        playerBet = 0;
        aiBet = 0;
        pot = 0;
        currentBet = 50;
        playerFolded = false;
        aiFolded = false;
        ClearCards();
        playerHand.Clear();
        aiHand.Clear();
        communityCards.Clear();
        deck.ResetDeck();
        roundStep = 0;
        PostBlinds();
        DealOpeningCards();
        waitingForPlayer = true;
        UpdateUI();
        Debug.Log("Round start");
    }

    private void NextStep()
    {
        if (roundStep == 0)
        {
            DealFlop();
            roundStep = 1;
            ResetBetsForNextRound();
            Debug.Log("flop dealt");
        }
        else if (roundStep == 1)
        {
            DealTurn();
            roundStep = 2;
            ResetBetsForNextRound();
            Debug.Log("turn dealt");
        }
        else if (roundStep == 2)
        {
            DealRiver();
            roundStep = 3;
            ResetBetsForNextRound();
            Debug.Log("river dealt");
        }
        else if (roundStep == 3)
        {
            SetMessage("Showdown");
            StartCoroutine(ShowdownRoutine());
        }
    }
    private void DealOpeningCards()
    {
        CardData playerCard1 = deck.Draw();
        CardData playerCard2 = deck.Draw();
        CardData aiCard1 = deck.Draw();
        CardData aiCard2 = deck.Draw();

        playerHand.Add(playerCard1);
        playerHand.Add(playerCard2);

        aiHand.Add(aiCard1);
        aiHand.Add(aiCard2);

        SpawnCard(playerCard1, handSlot1);
        SpawnCard(playerCard2, handSlot2);

        aiCardObj1 = SpawnCardFaceDown(aiHandSlot1);
        aiCardObj2 = SpawnCardFaceDown(aiHandSlot2);
    }
    private GameObject SpawnCardFaceDown(Transform slot)
    {
        GameObject newCard = Instantiate(cardPrefab, slot.position, Quaternion.identity);

        SpriteRenderer sr = newCard.GetComponent<SpriteRenderer>();
        sr.sprite = cardBackSprite;

        cardsOnTable.Add(newCard);
        return newCard;
    }
    private void RevealAICards()
    {
        if (aiHand.Count < 2)
        {
            return;
        }

        if (aiCardObj1 != null)
        {
            SpriteRenderer sr1 = aiCardObj1.GetComponent<SpriteRenderer>();
            sr1.sprite = aiHand[0].faceSprite;
        }

        if (aiCardObj2 != null)
        {
            SpriteRenderer sr2 = aiCardObj2.GetComponent<SpriteRenderer>();
            sr2.sprite = aiHand[1].faceSprite;
        }
    }
    private void SetMessage(string message)
    {
        if (gameMessageText != null)
        {
            gameMessageText.text = message;
        }
    }
    private void DealFlop()
    {
        CardData card1 = deck.Draw();
        CardData card2 = deck.Draw();
        CardData card3 = deck.Draw();

        communityCards.Add(card1);
        communityCards.Add(card2);
        communityCards.Add(card3);

        SpawnCard(card1, flop1Slot);
        SpawnCard(card2, flop2Slot);
        SpawnCard(card3, flop3Slot);
    }

    private void DealTurn()
    {
        CardData card = deck.Draw();
        communityCards.Add(card);
        SpawnCard(card, turnSlot);
    }

    private void DealRiver()
    {
        CardData card = deck.Draw();
        communityCards.Add(card);
        SpawnCard(card, riverSlot);
    }
    private void SpawnCard(CardData cardData, Transform slot)
    {
        GameObject newCard = Instantiate(cardPrefab, slot.position, Quaternion.identity);

        SpriteRenderer sr = newCard.GetComponent<SpriteRenderer>();
        sr.sprite = cardData.faceSprite;

        cardsOnTable.Add(newCard);
    }
    
    public void UpdateUI()
    {
        potText.text = "Pot: $" + pot;
        playerChipsText.text = "Player Chips: $" + playerChips;
        aiChipsText.text = "AI Chips: $" + aiChips;
    }

    private void ResetBetsForNextRound()
    {
        playerBet = 0;
        aiBet = 0;
        currentBet = 0;
        waitingForPlayer = true;
        UpdateUI();
    }

    private void PostBlinds()
    {
        int smallBlind = 25;
        int bigBlind = 50;

        playerChips -= smallBlind;
        playerBet = smallBlind;

        aiChips -= bigBlind;
        aiBet = bigBlind;

        pot = smallBlind + bigBlind;
        currentBet = bigBlind;

        UpdateUI();
    }

    private void AwardPotToPlayer()
    {
        playerChips += pot;
        Debug.Log("Player wins pot of " + pot);
        pot = 0;
        UpdateUI();
    }

    private void AwardPotToAI()
    {
        aiChips += pot;
        Debug.Log("AI wins pot of " + pot);
        pot = 0;
        UpdateUI();
    }

    private void SplitPot()
    {
        int splitAmount = pot / 2;
        playerChips += splitAmount;
        aiChips += splitAmount;
        Debug.Log("Pot split");
        pot = 0;
        UpdateUI();
    }
    public void EndPlayerTurn()
    {
        waitingForPlayer = false;
        bool continueRound = AITurn();

        if (continueRound)
        {
            NextStep();
        }
    }
    public void Fold()
    {
        playerFolded = true;
        SetMessage("Player folded");
        Debug.Log("Player folded");
        AwardPotToAI();
        waitingForPlayerResponseToRaise = false;
        Invoke(nameof(StartRound), 2f);
    }

    public void Call()
    {
        int amountToCall = currentBet - playerBet;
        if (amountToCall > playerChips)
        {
            amountToCall = playerChips;
        }

        playerChips -= amountToCall;
        playerBet += amountToCall;
        pot += amountToCall;
        UpdateUI();

        if (waitingForPlayerResponseToRaise)
        {
            waitingForPlayerResponseToRaise = false;
            playerLastAction = "Call";
            playerLastRaiseAmount = 0;
            SetMessage("Player matched the AI raise.:");
            NextStep();
            return;
        }
        SetMessage("Player called $" + amountToCall);
        Debug.Log("Player called: " + amountToCall);
        playerLastAction = "Call";
        playerLastRaiseAmount = 0;
        EndPlayerTurn();
    }

    public void OpenRaisePanel()
    {
        raisePanel.SetActive(true);
        raiseInputField.text = "";
    }

    public void ConfirmRaise()
    {
        int raiseAmount;
        if (!int.TryParse(raiseInputField.text, out raiseAmount))
        {
            SetMessage("Enter a valid number");
            return;
        }

        if (raiseAmount <= 0)
        {
            SetMessage("Raise must be more than 0");
            return;
        }

        int amountToCall = currentBet - playerBet;
        int totalAmount = amountToCall + raiseAmount;

        if (totalAmount > playerChips)
        {
            totalAmount = playerChips;
        }

        playerChips -= totalAmount;
        playerBet += totalAmount;
        pot += totalAmount;
        currentBet = playerBet;
        playerLastAction = "Raise";
        playerLastRaiseAmount = raiseAmount;

        UpdateUI();
        SetMessage("Player raised $" + raiseAmount);

        raisePanel.SetActive(false);
        EndPlayerTurn();
    }

    public void CancelRaise()
    {
        raisePanel.SetActive(false);
    }
    

    public bool AITurn()
    {
        List<CardData> aiCards = new List<CardData>();
        aiCards.AddRange(aiHand);
        aiCards.AddRange(communityCards);

        string aiHandType = HandEvaluator.EvaluateHand(aiCards);
        int handStrength = GetHandRank(aiHandType);
        int amountToCall = currentBet - aiBet;

        if (playerLastAction == "Raise")
        {
            if (handStrength <= 2 && playerLastRaiseAmount >= 100)
            {
                // weak hand -> fold or call
                if (Random.Range(0, 100) < 60)
                {
                    aiFolded = true;
                    SetMessage("AI folded");
                    Debug.Log("AI folded (weak hand)");
                    AwardPotToPlayer();
                    StartRound();
                    return false;
                }
            }
            else if (handStrength >= 6)
            {
                // strong hand -> raise more
                int raiseAmount = 50;
                int total = amountToCall + raiseAmount;

                if (total > aiChips)
                {
                    total = aiChips;
                }

                aiChips -= total;
                aiBet += total;
                pot += total;
                currentBet = aiBet;
                waitingForPlayerResponseToRaise = true;
                waitingForPlayer = true;

                SetMessage("AI raised $" + total + ". Call or fold to continue.");
                Debug.Log("AI raised (strong hand)");
                UpdateUI();
                return false;
            }
        }

        if (handStrength >= 4 && Random.Range(0, 100) < 30)
        {
            int raiseAmount = 50;
            int total = amountToCall + raiseAmount;

            if (total > aiChips)
            {
                total = aiChips;
            }
            aiChips -= total;
            aiBet += total;
            pot += total;
            currentBet = aiBet;
            waitingForPlayerResponseToRaise = true;
            waitingForPlayer = true;

            SetMessage("AI raised $" + total + ". Call or fold to continue.");
            UpdateUI();
            return false;
        }
        
        // default action -> CALL
        if (amountToCall > aiChips)
        {
            amountToCall = aiChips;
        }

        aiChips -= amountToCall;
        aiBet += amountToCall;
        pot += amountToCall;

        SetMessage("AI called $" + amountToCall);
        Debug.Log("AI called");
        UpdateUI();
        return true;

    }
    private int GetHandRank(string handName)
    {
        switch (handName)
        {
            case "High Card": return 1;
            case "Pair": return 2;
            case "Two Pair": return 3;
            case "Three of a Kind": return 4;
            case "Straight": return 5;
            case "Flush": return 6;
            case "Full House": return 7;
            case "Four of a Kind": return 8;
            case "Straight Flush": return 9;
            default: return 0;
        }
    }
    private void Showdown()
    {
        RevealAICards();
        List<CardData> playerCards = new List<CardData>();
        playerCards.AddRange(playerHand);
        playerCards.AddRange(communityCards);

        List<CardData> aiCards = new List<CardData>();
        aiCards.AddRange(aiHand);
        aiCards.AddRange(communityCards);

        string playerResult = HandEvaluator.EvaluateHand(playerCards);
        string aiResult = HandEvaluator.EvaluateHand(aiCards);

        Debug.Log("Player hand: " + playerResult);
        Debug.Log("AI hand: " + aiResult);

        int playerScore = GetHandRank(playerResult);
        int aiScore = GetHandRank(aiResult);

        if (playerScore > aiScore)
        {
            SetMessage("Player wins with " + playerResult);
            AwardPotToPlayer();
        }
        else if (aiScore > playerScore)
        {
            SetMessage("AI wins with " + aiResult);
            AwardPotToAI();
        }
        else
        {
            SetMessage("Tie hand");
            SplitPot();
        }

        Invoke(nameof(StartRound), 4f);
    }

    IEnumerator ShowdownRoutine()
    {
        Debug.Log("Showdown...");

        yield return new WaitForSeconds(1.5f);

        Showdown();
    }

    private void ClearCards()
    {
        for (int i = 0; i < cardsOnTable.Count; i++)
        {
            Destroy(cardsOnTable[i]);
        }
        cardsOnTable.Clear();
    }
}
