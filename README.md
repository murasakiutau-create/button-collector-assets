# button-collector-assets

ボタン収集ゲーム用の画像素材と、家具色替え工房ゲームのUnityプロジェクトを収録したリポジトリです。

---

## 家具色替え工房ゲーム（Unity WebGL）

`FurnitureColorGame/` フォルダに Unity 2022.3 LTS プロジェクトが入っています。
3D家具モデルをブラウザ上でカラーパレットを使って自由に塗り替えるゲームです。

### 必要環境

- [Unity Hub](https://unity.com/ja/download) + **Unity 2022.3.x LTS**（WebGL Build Support モジュール付き）

### セットアップ手順

1. Unity Hub を開き、**Add project from disk** → `FurnitureColorGame/` フォルダを選択
2. Unity Editor が起動し、`Library/` フォルダが自動生成されます（初回 1〜3 分）
3. **Window > Package Manager** で `glTFast 6.2.0` が「In Project」に表示されることを確認
   - 表示されない場合: Package Manager 右上 `+` > **Add package by name** → `com.unity.cloud.gltfast`

### 自作3Dモデルの追加方法

1. `.glb` ファイルを `FurnitureColorGame/Assets/StreamingAssets/models/` にコピー
2. `FurnitureColorGame/Assets/StreamingAssets/models_manifest.json` を編集してファイル名を追加:
   ```json
   {
     "models": ["your_chair.glb", "your_table.glb"]
   }
   ```
3. Unity Editor で **Play** → モデル選択ドロップダウンに追加したモデルが表示されます

### ゲームの遊び方

- 3Dモデルのパーツをクリックして選択（ハイライト表示）
- 画面下部のカラーパレットから色を選ぶと選択パーツに塗り替え

### WebGL ビルド手順

1. **File > Build Settings** → プラットフォームを **WebGL** に切り替え
2. **Player Settings**:
   - Company Name・Product Name を設定
   - WebGL タブ: Memory Size `256` MB 以上、Compression Format `Disabled`（開発時）
3. **Build and Run** → `Builds/WebGL/` に出力
4. ローカルでテストする場合はビルドフォルダで `python3 -m http.server` を実行
   （WebGL は `file://` プロトコルでは動作しません）

### トラブルシューティング

- **WebGL でモデルがピンク色になる**: Standardシェーダーが剥ぎ取られています。
  `Assets/Resources/FurnitureMaterials/StandardBase.mat` が存在することを確認してください。
- **モデルが表示されない**: `models_manifest.json` のファイル名がフォルダ内のファイルと一致しているか確認してください。
