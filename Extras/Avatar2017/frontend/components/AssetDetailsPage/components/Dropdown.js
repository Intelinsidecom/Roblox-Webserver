import Dropdown2016 from "../../dropdown2016";
import AssetDetailsStore from "../stores/AssetDetailsStore";
import { useEffect, useState } from "react";
import Authentication from "../../../stores/authentication";
import AssetDetailsModalStore from "../stores/AssetDetailsModalStore";

function AssetDropdown() {
    const auth = Authentication.useContainer();
    const store = AssetDetailsStore.useContainer();
    const modals = AssetDetailsModalStore.useContainer();
    const [options, setOptions] = useState(/** @type DropdownOption[] */([]));
    
    useEffect(() => {
        setOptions([
            store.details.creatorTargetId === auth.userId ? {
                name: "Configure",
                url: `/My/Item.aspx?id=${store.details.id}`,
            } : null,
            store.isResellable() && store.ownedCopies.filter(v => v.price === null || v.price === 0).length > 0 ? {
                name: "Sell",
                onClick: () => modals.setSellItemModalOpen(true),
            } : null,
            // store.isResellable() && store.resellers.find(v => v?.seller?.id === auth.userId) ? {
            //     name: "Delist",
            //     onClick: () => modals.setDelistItemModalOpen(true),
            // } : null,
            store.isCollectioned ? {
                name: "Remove from Profile",
                onClick: () => {
                    store.setCollectioned(false);
                    store.ToggleFromCollection(false).then();
                },
            } : store.ownedCopies?.length > 0 ? {
                name: "Add to Profile",
                onClick: () => {
                    store.setCollectioned(true);
                    store.ToggleFromCollection(true).then();
                },
            } : null,
            auth.isAuthenticated && (store.isOwned || store.ownedCopies?.length > 0) && !store.isResellable() ? {
                name: "Remove from Inventory",
                onClick: () => modals.setRemoveInvModalOpen(true),
            } : null,
            store.details.creatorTargetId === auth.userId ? {
                name: "Advertise",
                url: `/My/CreateUserAd.aspx?targetId=${store.details.id}&targetType=asset`,
            } : null,
            {
                name: "Report Item",
                url: `/abusereport/asset?id=${store.details.id}&RedirectUrl=${encodeURIComponent(window.location.href)}`,
            },
        ].filter(v => !!v));
    }, [
        store.details,
        store.resaleData,
        store.isCollectioned,
        store.ownedCopies,
        store.resellers,
        store.isOwned,
    ]);
    
    return <Dropdown2016
        options={options}
        closeOnClick={true}
    />
}

export default AssetDropdown;
