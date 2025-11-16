import React, { useState, useEffect } from "react";
import { createUseStyles } from "react-jss";
import AdBanner from "../../components/ad/adBanner";
import CharacterPage from "../../components/characterCustomizerPage";
import MyAvatar from "../../components/characterCustomizerPage/components/avatar";
import CharacterCustomizationStore from "../../stores/characterPage";

const useCharacterPageStyles = createUseStyles({
  header: {
    fontWeight: 700,
    marginBottom: 0,
    marginTop: 0,
    fontSize: '30px',
  },
  characterContainer: {
    position: 'relative',
    background: 'var(--white-color)',
    padding: '4px 8px',
    overflow: 'hidden',
  },
  switchButton: {
    position: 'absolute',
    top: '10px',
    right: '10px',
  },
});

const MyCharacterPage = props => {
  const s = useCharacterPageStyles();
  const [rigType, setRigType] = useState(null);

  useEffect(() => {
    fetch('/apisite/avatar/v1/avatar') // r15 - r6 checker
      .then(response => {
        if (!response.ok) {
          throw new Error('Failed to fetch avatar data');
        }
        return response.json();
      })
      .then(data => {
        const avatarType = data.playerAvatarType;
        console.log(data)
        if (avatarType === 'R6' || avatarType === 'R15') {
          setRigType(avatarType);
        } else {
          throw new Error('Invalid avatar type');
        }
      })
      .catch(error => {
        console.error('Error fetching avatar data:', error);
      });
  }, []);

  const handleRigToggle = () => {
    const newRigType = rigType === "R6" ? "R15" : "R6";
    const apiUrl = `/apisite/avatar/v1/avatar/set-rig?rigtype=${newRigType}`;
    fetch(apiUrl)
      .then(response => {
        if (!response.ok) {
          throw new Error('Failed to set rig type');
        }
        setRigType(newRigType);
      })
      .catch(error => {
        console.error('Error setting rig type:', error);
      });
  };

  return (
    <div className="container">
      <AdBanner context="MyCharacterPage" />
      <div className={s.characterContainer}>
        <div className="row mt-2">
          <div className="col-12 ps-4 pe-4">
            <h1 className={s.header}>Character Customizer</h1>
            {rigType && (
              <button className={s.switchButton} onClick={handleRigToggle}>
                {rigType === "R6" ? "Switch to R15" : "Switch to R6"}
              </button>
            )}
          </div>
        </div>
        {rigType && (
          <CharacterCustomizationStore.Provider 
// @ts-ignore
          value={{ rigType }}>
            <CharacterPage />
          </CharacterCustomizationStore.Provider>
        )}
      </div>
    </div>
  );
};

export default MyCharacterPage;
