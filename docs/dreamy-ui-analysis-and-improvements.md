# Dreamy UI: Phan tich hien trang va de xuat cai tien

## Pham vi

Tai lieu danh gia `com.dreamy.ui` version `0.1.1` dang duoc resolve tai
`Library/PackageCache/com.dreamy.ui@133795d42723`, tap trung vao panel lifecycle,
tween lifecycle va flow sinh item dong cua `UIShopPanel`.

## Kien truc hien tai

- `PanelManager` quan ly layer, stack, create/show/hide va transition.
- `UIPanel` quan ly state, cancellation token, CanvasGroup va `TweenPlayer`.
- `TweenPlayer` quet tat ca `ITween` trong hierarchy, init va chay tween auto.
- `UITweenBase` dong goi settings, delay, lifecycle DOTween va visual state.
- `TweenDelayControl` ap delay theo index cho cac item dong.

Huong tach trach nhiem nay hop ly: panel khong can biet chi tiet DOTween, con tween
component co the tai su dung tren prefab.

## Van de da xac nhan

### 1. Item dong bi visible roi bi an

`Pools.Spawn` kich hoat item truoc. Sau khi tat ca item duoc tao,
`UIShopPanel` goi `TweenPlayer.Init()`. `UITweenBase.Init()` lai goi `Inactive()`,
nen `UITweenScale` dat scale ve `0` ngay sau khi item da duoc spawn.

Day la contract lifecycle khong ro rang: `Init` vua bind dependency, vua thay doi
visual state. Caller khong the khoi tao tween ma khong tao side effect.

### 2. Dung pool de spawn nhung dung Destroy de don

Flow cu dung `Pools.Spawn` nhung don item bang `Destroy`. Ngoai viec khong tra item
ve pool, `Destroy` chi co hieu luc cuoi frame. Lan quet `GetComponentsInChildren`
ngay sau do co the thu ca tween cua item cu dang cho huy va tween cua item moi.
Danh sach tween giu reference het han la nguon truc tiep cua log null lien quan
`CanvasGroup` va `UITweenScale`.

### 3. Re-init khong thuc su reset tween

`TweenPlayer.Init()` quet lai hierarchy, nhung moi `UITweenBase` co co
`isInitialized`. Voi item lay lai tu pool, `Init()` khong dua item ve initial state
lan nua. Ket qua phu thuoc vao state con lai tu lan despawn truoc.

### 4. Contract dependency chua nhat quan

`UITweenFade` tu phuc hoi `CanvasGroup` trong `Setup`, nhung `UITweenScale` su dung
thang `transform`. Guard null nam rai rac va dependency sai chi bi bo qua im lang,
lam prefab hong kho phat hien.

## Thay doi trong project

`UIShopPanel` da duoc don gon thanh mot pipeline dung chung:

1. Despawn toan bo item cu ngay lap tuc va tra ve dung pool.
2. Dung `PopulateOffers` cho ca Gem va Gold.
3. Bind data ngay sau spawn.
4. Ap stagger delay sau khi danh sach cua holder da day du.
5. Quet/init tween sau cung, khong con quet item dang cho `Destroy`.

Flow moi loai bo code lap va nguyen nhan reference null do item cu. Panel dang
inactive trong `OnPreShow`, nen intermediate state do `Init -> Inactive` khong
duoc render truoc show tween.

## De xuat cai tien package

### Uu tien P0

- Tach `UITweenBase.Initialize()` khoi `SetInitialState(TweenDirection)`.
  Khoi tao dependency khong nen thay doi visual.
- Them `TweenPlayer.Refresh(bool resetState)` cho hierarchy dong, thay vi tai su
  dung `Init()` voi hai nghia.
- Them lifecycle pool cho tween: `OnSpawn` reset state, `OnDespawn` kill tween va
  xoa delay override.
- Loc Unity object da bi destroy/null truoc moi lan play va kill.

### Uu tien P1

- Validate dependency trong `OnValidate` va log mot loi co context.
- De `TweenDelayControl` tra ve danh sach tween da ap delay, giup test thu tu.
- Quy dinh mot owner duy nhat cho `interactable`/`blocksRaycasts`.
- Truyen cancellation token truc tiep vao `ITween.Show/Hide`.

### Uu tien P2

- Them sample chinh thuc cho dynamic list, pooling va stagger.
- Them asmdef test rieng cho EditMode va PlayMode.
- Them XML documentation cho thu tu `Init`, `PostInit`, `Show`, `Hide` va events.

## API de xuat

```csharp
public interface ITween
{
    void Initialize();
    void ResetTo(TweenVisualState state);
    UniTask PlayIn(CancellationToken token);
    UniTask PlayOut(CancellationToken token);
    void Kill(bool complete = false);
}

public sealed class TweenPlayer
{
    public void Refresh();
    public void ResetTo(TweenVisualState state);
    public UniTask PlayIn(CancellationToken token);
    public UniTask PlayOut(CancellationToken token);
}
```

API nay tach ba viec dang bi tron trong `Init`: discover component, bind
dependency va thay doi visual state.

## Test can bo sung

- Dynamic item khong visible mot frame truoc animation.
- Refresh sau despawn khong giu tween cua item cu.
- Item lay lai tu pool luon bat dau tu initial state.
- Hide bi cancel giua chung khong de CanvasGroup/scale o state trung gian.
- Prefab thieu CanvasGroup bao loi co GameObject path ro rang.
- Show/Hide lien tuc khong phat sinh tween orphan hoac `NullReferenceException`.
