import { useRouter } from "next/dist/client/router";
import React, { useEffect } from "react";

const CanManagePage = props => {
  const router = useRouter();

  useEffect(() => {
    const { userId, placeId } = router.query;
    if (userId && placeId) {
      router.push(`/game/players/${userId}/canmanage/${placeId}`);
    }
  }, [router]);
  return null; 
};

export default CanManagePage;
