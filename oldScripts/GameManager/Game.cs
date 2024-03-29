﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private SEManager SE;
    [SerializeField] private EffectManager effect;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private int PickelDamage=5;
    [SerializeField] private int HammerDamage=10;
    [SerializeField] private int BombDamage=20;
    [SerializeField] private int MissDamage = 10;
    
    private ShakeCamera _camera;

    [SerializeField] private GameObject ClearText;
    [SerializeField] private GameObject GameOverText;
    
    [SerializeField]
    private Board board;

    private BoardHP boardHp;

    DigItem _digItem = new DigItem();


    public bool IsGameStoped;
    public bool IsFinished { get; private set; } = false;
    
    public DigItemEnum usingItemEnum = DigItemEnum.Pickel;

    private void Start()
    {
        boardHp = board.boardHP;
        _camera = mainCamera.GetComponent<ShakeCamera>();
    }

    public void Dig(Tile tile)
    {
        int sumMissDamage = 0;
        
        Tile[,] tiles = board.boardTileArray2d;
        DigItem.ItemInfo itemInfo = _digItem.GetItemInfo(usingItemEnum);
        for (int i = 0; i < itemInfo.Length; i++)
        {
            bool limitColumn = tile.Column + itemInfo.x[i] >= board.Columns || tile.Column + itemInfo.x[i] < 0;
            bool limitRow = tile.Row + itemInfo.y[i] >= board.Rows || tile.Row + itemInfo.y[i] < 0;
            if( limitColumn || limitRow)
                continue;
            
            var targetTile = tiles[tile.Column + itemInfo.x[i], tile.Row + itemInfo.y[i]];
            if (targetTile.HP.isOverDamaged(itemInfo.value[i]))
            {
                boardHp.Subtract(MissDamage);
                sumMissDamage += MissDamage;
            }

            targetTile.HP.Subtract(itemInfo.value[i]);


        }

        switch (usingItemEnum)
        {
            case DigItemEnum.Pickel:boardHp.Subtract(PickelDamage);
                break;
            case DigItemEnum.Hammer:boardHp.Subtract(HammerDamage);
                break;
            case DigItemEnum.Bomb:boardHp.Subtract(BombDamage);
                break;
            default: break;
        }

        float addvalue = sumMissDamage / (float)MissDamage;
        _camera.Shake(addvalue,addvalue);
        effect.InstantiateEffect(usingItemEnum,tile.gameObject.transform);
        SE.PlaySE(usingItemEnum);

        if (CheckGameEnd())
        {
            ClearText.SetActive(true);
            IsFinished = true;
            return;
        }

        if (boardHp.BoardHp == 0)
        {
            //GameOver処理
            GameOverText.SetActive(true);
            IsFinished = true;
            return;
        }
    }


    public void SetActiveTargetTile(Tile tile,bool active)
    {
        Tile[,] tiles = board.boardTileArray2d;
        DigItem.ItemInfo itemInfo = _digItem.GetItemInfo(usingItemEnum);
        for (int i = 0; i < itemInfo.Length; i++)
        {
            bool limitColumn = tile.Column + itemInfo.x[i] >= board.Columns || tile.Column + itemInfo.x[i] < 0;
            bool limitRow = tile.Row + itemInfo.y[i] >= board.Rows || tile.Row + itemInfo.y[i] < 0;
            if (limitColumn || limitRow)
                continue;

            var targetTile = tiles[tile.Column + itemInfo.x[i], tile.Row + itemInfo.y[i]];

            if (active)
            {
                targetTile.SetTrueToChooseTile();
            }
            else
            {
                targetTile.SetFalseToChooseTile();
            }

        }
    }

    private bool CheckGameEnd()
    {
        Tile[,] tiles = board.boardTileArray2d;
        foreach (var tile in tiles)
        {
            if (tile.isUnderItem && tile.HP.GetHP() > 0)
                return false;
        }
        
        return true;
    }
}
